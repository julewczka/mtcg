using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using mtcg.classes.entities;
using mtcg.repositories;

namespace mtcg.controller
{
    public class TradingController
    {
        private static readonly object TradingLock = new();
        private readonly TradingRepository _tradingRepo;
        private readonly CardRepository _cardRepo;
        private readonly DeckRepository _deckRepo;

        public TradingController()
        {
            _tradingRepo = new TradingRepository();
            _cardRepo = new CardRepository();
            _deckRepo = new DeckRepository();
        }

        public Response Get(IReadOnlyList<string> resource)
        {
            var content = new StringBuilder();
            switch (resource.Count)
            {
                case 1:
                    var fetchedTradings = _tradingRepo.GetAllDeals();
                    if (fetchedTradings.Count <= 0)
                        return RTypes.CError("No trading deals at the moment!", 404);
                    fetchedTradings.ForEach(trading =>
                        content.Append(JsonSerializer.Serialize(trading) + "," + Environment.NewLine));
                    break;
                case 2:
                    var fetchedTrading = _tradingRepo.GetDealByUuid(resource[1]);
                    if (fetchedTrading?.Uuid == null) return RTypes.NotFoundRequest;
                    content.Append(JsonSerializer.Serialize(fetchedTrading) + "," + Environment.NewLine);
                    break;
                default:
                    return RTypes.BadRequest;
            }

            return RTypes.CResponse(content.ToString(), 200, "application/json");
        }

        //TODO: Add PUT method
        public Response Delete(User user, string tradingUuid)
        {
            var trading = _tradingRepo.GetDealByUuid(tradingUuid);
            if (trading?.Uuid == null) return RTypes.NotFoundRequest;
            if (user.Id != trading.Trader) return RTypes.Forbidden;
            return _tradingRepo.DeleteDealByUuid(trading.Uuid) ? RTypes.HttpOk : RTypes.BadRequest;
        }

        public Response Post(User user, IReadOnlyList<string> resource, string payload)
        {
            if (user?.Id == null) return RTypes.Forbidden;

            switch (resource.Count)
            {
                case 1:
                    var trading = JsonSerializer.Deserialize<Trading>(payload);
                    if (trading?.Uuid == null) return RTypes.BadRequest;
                    trading.Trader = user.Id;
                    var offeredCard = _cardRepo.GetByUuid(trading.CardToTrade);
                    var stack = StackRepository.SelectStackByUserId(trading.Trader);
                    var validate = CheckDeal(offeredCard, stack);
                    if (validate != null) return validate;

                    lock (TradingLock)
                    {
                        if (!_tradingRepo.AddDeal(trading)) return RTypes.BadRequest;
                        StackController.AddToLockList(offeredCard);
                    }

                    break;
                case 2:
                    var cleanPayload = payload.Replace("\"", string.Empty);
                    var checkTrade = CheckTrade(resource[1], cleanPayload, user);
                    if (checkTrade != null) return checkTrade;
                    
                    lock (TradingLock)
                    {
                        if (!_tradingRepo.BeginTrade(resource[1], cleanPayload, user))
                            return RTypes.CError("transaction failed", 400);
                        
                        var cardToTrade = _cardRepo.GetByUuid(cleanPayload);
                        StackController.RemoveFromLockList(cardToTrade);
                    }
                    
                    break;
                default:
                    return RTypes.BadRequest;
            }

            return RTypes.Created;
        }

        private Response CheckDeal(Card offeredCard, Stack stack)
        {
            if (offeredCard?.Uuid == null) return RTypes.NotFoundRequest;
            if (stack?.Uuid == null) return RTypes.Forbidden;
            if (!StackRepository.IsCardInStack(offeredCard.Uuid, stack.Uuid)) return RTypes.Forbidden;
            if (_deckRepo.IsCardInDeck(offeredCard.Uuid)) return RTypes.Forbidden;
            return PackageRepository.IsCardInPackages(offeredCard.Uuid) ? RTypes.Forbidden : null;
        }

        private Response CheckTrade(string tradingUuid, string cardUuid, User newOwner)
        {
            var tradingRepository = new TradingRepository();
            //check if trading deal exists
            var trading = tradingRepository.GetDealByUuid(tradingUuid);
            if (trading?.Uuid == null) return RTypes.NotFoundRequest;
            if (trading?.Trader == null) return RTypes.NotFoundRequest;
            if (trading?.CardToTrade == null) return RTypes.NotFoundRequest;

            //check if offered card exists
            var card = _cardRepo.GetByUuid(cardUuid);
            if (card?.Uuid == null) return RTypes.Forbidden;

            //check if trader is the same as the buyer
            if (newOwner?.Id == null) return RTypes.Unauthorized;
            if (trading.Trader.Equals(newOwner.Id, StringComparison.CurrentCultureIgnoreCase))
                return RTypes.BadRequest;

            //check if buyer has a stack
            var newOwnerStack = StackRepository.SelectStackByUserId(newOwner.Id);
            if (newOwnerStack?.Uuid == null) return RTypes.CError("Buyer has no stack", 404);

            //check if Card is in stack, deck or package
            if (!StackRepository.IsCardInStack(cardUuid, newOwnerStack.Uuid))
                return RTypes.CError("Card must be in stack", 403);
            if (PackageRepository.IsCardInPackages(cardUuid))
                return RTypes.CError("Card mustn't be in package", 403);
            if (_deckRepo.IsCardInDeck(cardUuid)) return RTypes.CError("Card mustn't be in deck", 403);

            //check if offered card meets requirements
            if (!trading.CardType.Equals(card.CardType, StringComparison.CurrentCultureIgnoreCase))
                return
                    RTypes.CError($"offered card have to be type of {trading.CardType}", 403);
            if (trading.MinimumDamage >= card.Damage)
                return RTypes.CError(
                    $"offered card's damage must be higher than required {trading.MinimumDamage}", 403);
            return null;
        }
    }
}