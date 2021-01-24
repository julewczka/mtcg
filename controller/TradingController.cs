using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class TradingController
    {
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

        public static Response Post(string token, IReadOnlyList<string> resource, string payload)
        {
            switch (resource.Count)
            {
                case 1:
                    Console.WriteLine($"Test Start!");
                    var trading = JsonSerializer.Deserialize<Trading>(payload);
                    if (trading?.Uuid == null) return ResponseTypes.BadRequest;
                    Console.WriteLine($"trading-uuid:{trading.Uuid}");

                    var user = UserRepository.SelectUserByToken(token);
                    trading.Trader = user.Id;
                    
                    var offeredCard = CardRepository.SelectCardByUuid(trading.CardToTrade);
                    if (offeredCard?.Uuid == null) return ResponseTypes.BadRequest;

                    Console.WriteLine($"offeredCard-uuid:{offeredCard.Uuid}");

                    var stack = StackRepository.GetStackByUserId(trading.Trader);
                    if (stack?.Uuid == null) return ResponseTypes.BadRequest;

                    Console.WriteLine($"trading-uuid:{stack.Uuid}");

                    if (!StackRepository.IsCardInStack(offeredCard.Uuid, stack.Uuid)) return ResponseTypes.Forbidden;
                    Console.WriteLine("Card is in Stack");

                    if (DeckRepository.IsCardInDeck(offeredCard.Uuid)) return ResponseTypes.Forbidden;
                    Console.WriteLine("Card is not in Deck");

                    if (PackageRepository.IsCardInPackages(offeredCard.Uuid)) return ResponseTypes.Forbidden;
                    Console.WriteLine("Card is not in Package");

                    
                    Console.WriteLine("Test");
                    
                    if (!TradingRepository.InsertTradingDeal(trading)) return ResponseTypes.BadRequest;
                    break;
                case 2:
                    Console.WriteLine($"payload #1: {payload}");
                    Console.WriteLine($"Test WL");
                    var cleanPayload = payload.Replace("\"", string.Empty);
                    Console.WriteLine($"payload #2: {cleanPayload}");
                    Console.WriteLine($"StartTest");
                    if (!ValidateTrade(resource[1], cleanPayload, token)) return ResponseTypes.BadRequest;
                    Console.WriteLine("MiddleTest");
                    if (!TradingRepository.StartToTrade(resource[1], cleanPayload, token)) return ResponseTypes.BadRequest;
                    Console.WriteLine("EndTest");
                    break;
                default:
                    return ResponseTypes.BadRequest;
            }

            return ResponseTypes.Created;
        }
        
        private static bool ValidateTrade(string tradingUuid, string cardUuid,string token)
        {
            var trading = TradingRepository.GetDealByUuid(tradingUuid);
            if (trading?.Uuid == null) return false;
            if (trading?.Trader == null) return false;
            if (trading?.CardToTrade == null) return false;
            
            var card = CardRepository.SelectCardByUuid(cardUuid);
            if (card?.Uuid == null) return false;
        
            var newOwner = UserRepository.SelectUserByToken(token);
            if (newOwner?.Id == null) return false;
            if (trading.Trader.Equals(newOwner.Id, StringComparison.CurrentCultureIgnoreCase)) return false;
            var newOwnerStack = StackRepository.GetStackByUserId(newOwner.Id);
            if (newOwnerStack?.Uuid == null) return false;
            if (!StackRepository.IsCardInStack(cardUuid, newOwnerStack.Uuid)) return false;
            if (PackageRepository.IsCardInPackages(cardUuid)) return false;
            if (DeckRepository.IsCardInDeck(cardUuid)) return false;
            return /*trading.CardType == card.CardType &&*/ trading.MinimumDamage <= card.Damage;
        }
    }
}