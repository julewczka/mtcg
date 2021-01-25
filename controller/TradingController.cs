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
            switch (resource.Count)
            {
                case 1:
                    var fetchedTradings = TradingRepository.GetAllDeals();
                    if (fetchedTradings.Count <= 0)
                        return ResponseTypes.CustomError("No trading deals at the moment!", 404);
                    fetchedTradings.ForEach(trading =>
                        content.Append(JsonSerializer.Serialize(trading) + "," + Environment.NewLine));
                    break;
                case 2:
                    var fetchedTrading = TradingRepository.GetDealByUuid(resource[1]);
                    if (fetchedTrading?.Uuid == null) return ResponseTypes.NotFoundRequest;
                    content.Append(JsonSerializer.Serialize(fetchedTrading) + "," + Environment.NewLine);
                    break;
                default:
                    return ResponseTypes.BadRequest;
            }

            return ResponseTypes.CustomResponse(content.ToString(), 200, "application/json");
        }

        public static Response Delete(string token, string tradingUuid)
        {
            var trading = TradingRepository.GetDealByUuid(tradingUuid);
            if (trading?.Uuid == null) return ResponseTypes.NotFoundRequest;
            var user = UserRepository.SelectUserByToken(token);
            if (user.Id != trading.Trader) return ResponseTypes.Forbidden;
            return TradingRepository.DeleteTrading(trading.Uuid) ? ResponseTypes.HttpOk : ResponseTypes.BadRequest;
        }

        public static Response Post(string token, IReadOnlyList<string> resource, string payload)
        {
            switch (resource.Count)
            {
                case 1:
                    var user = UserRepository.SelectUserByToken(token);
                    var trading = JsonSerializer.Deserialize<Trading>(payload);
                    if (trading?.Uuid == null) return ResponseTypes.BadRequest;
                    trading.Trader = user.Id;
                    var offeredCard = CardRepository.SelectCardByUuid(trading.CardToTrade);
                    var stack = StackRepository.GetStackByUserId(trading.Trader);
                    var validate = ValidateRequestedDeal(token, offeredCard, stack);
                    if (validate != null) return validate;

                    lock (TradingLock)
                    {
                        if (!TradingRepository.InsertTradingDeal(trading)) return ResponseTypes.BadRequest;
                        StackController.AddToLockList(offeredCard);
                    }

                    break;
                case 2:
                    var cleanPayload = payload.Replace("\"", string.Empty);
                    var validateTrade = ValidateTrade(resource[1], cleanPayload, token);
                    if (validateTrade != null) return validateTrade;
                    
                    lock (TradingLock)
                    {
                        if (!TradingRepository.StartToTrade(resource[1], cleanPayload, token))
                            return ResponseTypes.BadRequest;
                        var cardToTrade = CardRepository.SelectCardByUuid(cleanPayload);
                        StackController.RemoveFromLockList(cardToTrade);
                    }
                    
                    break;
                default:
                    return ResponseTypes.BadRequest;
            }

            return ResponseTypes.Created;
        }

        private static Response ValidateRequestedDeal(string token, Card offeredCard, Stack stack)
        {
            if (UserRepository.SelectUserByToken(token).Id == null) return ResponseTypes.Forbidden;
            if (offeredCard?.Uuid == null) return ResponseTypes.NotFoundRequest;
            if (stack?.Uuid == null) return ResponseTypes.Forbidden;
            if (!StackRepository.IsCardInStack(offeredCard.Uuid, stack.Uuid)) return ResponseTypes.Forbidden;
            if (DeckRepository.IsCardInDeck(offeredCard.Uuid)) return ResponseTypes.Forbidden;
            return PackageRepository.IsCardInPackages(offeredCard.Uuid) ? ResponseTypes.Forbidden : null;
        }

        private static Response ValidateTrade(string tradingUuid, string cardUuid, string token)
        {
            //check if trading deal exists
            var trading = TradingRepository.GetDealByUuid(tradingUuid);
            if (trading?.Uuid == null) return ResponseTypes.NotFoundRequest;
            if (trading?.Trader == null) return ResponseTypes.NotFoundRequest;
            if (trading?.CardToTrade == null) return ResponseTypes.NotFoundRequest;

            //check if offered card exists
            var card = CardRepository.SelectCardByUuid(cardUuid);
            if (card?.Uuid == null) return ResponseTypes.Forbidden;

            //check if trader is the same as the buyer
            var newOwner = UserRepository.SelectUserByToken(token);
            if (newOwner?.Id == null) return ResponseTypes.Unauthorized;
            if (trading.Trader.Equals(newOwner.Id, StringComparison.CurrentCultureIgnoreCase))
                return ResponseTypes.BadRequest;

            //check if buyer has a stack
            var newOwnerStack = StackRepository.GetStackByUserId(newOwner.Id);
            if (newOwnerStack?.Uuid == null) return ResponseTypes.CustomError("Buyer has no stack", 404);

            //check if Card is in stack, deck or package
            if (!StackRepository.IsCardInStack(cardUuid, newOwnerStack.Uuid))
                return ResponseTypes.CustomError("Card must be in stack", 403);
            if (PackageRepository.IsCardInPackages(cardUuid))
                return ResponseTypes.CustomError("Card mustn't be in package", 403);
            if (DeckRepository.IsCardInDeck(cardUuid)) return ResponseTypes.CustomError("Card mustn't be in deck", 403);

            //check if offered card meets requirements
            if (!trading.CardType.Equals(card.CardType, StringComparison.CurrentCultureIgnoreCase))
                return
                    ResponseTypes.CustomError($"offered card have to be type of {trading.CardType}", 403);
            if (trading.MinimumDamage <= card.Damage)
                return ResponseTypes.CustomError(
                    $"offered card's damage must be higher than required {trading.MinimumDamage}", 403);
            return null;
        }
    }
}