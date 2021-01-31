using System;
using System.Text;
using System.Text.Json;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class DeckController
    {
        public static Response GetDeckByToken(string token)
        {
            var user = UserRepository.SelectUserByToken(token);
            if (user == null) return RTypes.CError("user not found", 404);

            var deck = DeckRepository.GetDeckByUserUuid(user.Id);
            if (deck?.Uuid == null) return RTypes.CError("deck not found - create one", 404);

            deck.Cards = DeckRepository.GetCardsFromDeck(deck.Uuid);

            var content = new StringBuilder(JsonSerializer.Serialize(deck));
            return RTypes.CResponse(content.ToString(), 200, "application/json");
        }

        public static Response CreateDeck(string token, string cardUuids)
        {
            if (GetDeckByToken(token).StatusCode == 200) return RTypes.CError("Deck already created", 405);
            
            var removeBrackets =
                cardUuids
                    .Remove(0, 1)
                    .Remove(cardUuids.Length - 2, 1)
                    .Replace("\"", string.Empty);

            var cardUuidsAsArray = removeBrackets.Split(", ");

            var data = new StringBuilder();

            if (cardUuidsAsArray.Length < 4) return RTypes.BadRequest;
            var deck = DeckRepository.ConfigureDeck(token, cardUuidsAsArray);

            if (deck == null) return RTypes.NotFoundRequest;
            deck.Cards.ForEach(card => data.Append(JsonSerializer.Serialize(card) + "," + Environment.NewLine));

            return RTypes.CResponse(data.ToString(), 200, "application/json");
        }

        public static Response ConfigureDeck(string token, string cardUuids)
        {
            var data = new StringBuilder();
            var removeBrackets =
                cardUuids
                    .Remove(0, 1)
                    .Remove(cardUuids.Length - 2, 1)
                    .Replace("\"", string.Empty);

            var cardUuidsAsArray = removeBrackets.Split(", ");
            if (cardUuidsAsArray.Length < 4) return RTypes.CError("you must insert 4 id's", 400);

            foreach (var cardUuid in cardUuidsAsArray)
            {
                var card = CardRepository.SelectCardByUuid(cardUuid);
                if (card?.Uuid == null) return RTypes.Forbidden;

                if (StackController.IsLocked(card)) return RTypes.CError($"{card.Uuid} is locked!", 403);
            }
            
            var deck = DeckRepository.ConfigureDeck(token, cardUuidsAsArray);
            if (deck == null) return RTypes.NotFoundRequest;
            
            deck.Cards.ForEach(card => data.Append(JsonSerializer.Serialize(card) + "," + Environment.NewLine));

            return RTypes.Created;
        }
    }
}