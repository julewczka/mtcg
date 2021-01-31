using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using mtcg.classes.entities;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class TradingController
    {
        private static readonly object TradingLock = new();

        public static Response Get(IReadOnlyList<string> resource)
        {
            var content = new StringBuilder();
            var tradingRepository = new TradingRepository();
            switch (resource.Count)
            {
                case 1:
                    var fetchedTradings = tradingRepository.GetAllDeals();
                    if (fetchedTradings.Count <= 0)
                        return RTypes.CError("No trading deals at the moment!", 404);
                    fetchedTradings.ForEach(trading =>
                        content.Append(JsonSerializer.Serialize(trading) + "," + Environment.NewLine));
                    break;
                case 2:
                    var fetchedTrading = tradingRepository.GetDealByUuid(resource[1]);
                    if (fetchedTrading?.Uuid == null) return RTypes.NotFoundRequest;
                    content.Append(JsonSerializer.Serialize(fetchedTrading) + "," + Environment.NewLine);
                    break;
                default:
                    return RTypes.BadRequest;
            }

            return RTypes.CResponse(content.ToString(), 200, "application/json");
        }

        
        public static Response Delete(string token, string tradingUuid)
        {
            var tradingRepository = new TradingRepository();
            var trading = tradingRepository.GetDealByUuid(tradingUuid);
            if (trading?.Uuid == null) return RTypes.NotFoundRequest;
            var user = UserRepository.SelectUserByToken(token);
            if (user.Id != trading.Trader) return RTypes.Forbidden;
            return tradingRepository.DeleteDealByUuid(trading.Uuid) ? RTypes.HttpOk : RTypes.BadRequest;
        }

        public static Response Post(string token, IReadOnlyList<string> resource, string payload)
        {
            var tradingRepository = new TradingRepository();
            switch (resource.Count)
            {
                case 1:
                    var user = UserRepository.SelectUserByToken(token);
                    var trading = JsonSerializer.Deserialize<Trading>(payload);
                    
                    if (trading?.Uuid == null) return RTypes.BadRequest;
                    trading.Trader = user.Id;
                    var offeredCard = CardRepository.SelectCardByUuid(trading.CardToTrade);
                    var stack = StackRepository.SelectStackByUserId(trading.Trader);
                    var validate = CheckDeal(token, offeredCard, stack);
                    if (validate != null) return validate;

                    lock (TradingLock)
                    {
                        if (!tradingRepository.AddDeal(trading)) return RTypes.BadRequest;
                        StackController.AddToLockList(offeredCard);
                    }

                    break;
                case 2:
                    var cleanPayload = payload.Replace("\"", string.Empty);
                    var validateTrade = CheckTrade(resource[1], cleanPayload, token);
                    if (validateTrade != null) return validateTrade;
                    
                    lock (TradingLock)
                    {
                        if (!tradingRepository.BeginTrade(resource[1], cleanPayload, token))
                            return RTypes.CError("transaction failed", 400);
                        
                        var cardToTrade = CardRepository.SelectCardByUuid(cleanPayload);
                        StackController.RemoveFromLockList(cardToTrade);
                    }
                    
                    break;
                default:
                    return RTypes.BadRequest;
            }

            return RTypes.Created;
        }

        private static Response CheckDeal(string token, Card offeredCard, Stack stack)
        {
            if (UserRepository.SelectUserByToken(token).Id == null) return RTypes.Forbidden;
            if (offeredCard?.Uuid == null) return RTypes.NotFoundRequest;
            if (stack?.Uuid == null) return RTypes.Forbidden;
            if (!StackRepository.IsCardInStack(offeredCard.Uuid, stack.Uuid)) return RTypes.Forbidden;
            if (DeckRepository.IsCardInDeck(offeredCard.Uuid)) return RTypes.Forbidden;
            return PackageRepository.IsCardInPackages(offeredCard.Uuid) ? RTypes.Forbidden : null;
        }

        private static Response CheckTrade(string tradingUuid, string cardUuid, string token)
        {
            var tradingRepository = new TradingRepository();
            //check if trading deal exists
            var trading = tradingRepository.GetDealByUuid(tradingUuid);
            if (trading?.Uuid == null) return RTypes.NotFoundRequest;
            if (trading?.Trader == null) return RTypes.NotFoundRequest;
            if (trading?.CardToTrade == null) return RTypes.NotFoundRequest;

            //check if offered card exists
            var card = CardRepository.SelectCardByUuid(cardUuid);
            if (card?.Uuid == null) return RTypes.Forbidden;

            //check if trader is the same as the buyer
            var newOwner = UserRepository.SelectUserByToken(token);
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
            if (DeckRepository.IsCardInDeck(cardUuid)) return RTypes.CError("Card mustn't be in deck", 403);

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