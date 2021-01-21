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
            if (user == null) return ResponseTypes.CustomError("user not found", 404);

            var deck = DeckRepository.GetDeckByUserUuid(user.Id);
            if (deck?.Uuid == null) return ResponseTypes.CustomError("deck not found - create one", 404);

            deck.Cards = DeckRepository.GetCardsFromDeck(deck.Uuid);

            var content = new StringBuilder(JsonSerializer.Serialize(deck));
            return ResponseTypes.CustomResponse(content.ToString(), 200, "application/json");
        }

        public static Response CreateDeck(string token, string cardUuids)
        {
            if (GetDeckByToken(token).StatusCode == 200) return ResponseTypes.CustomError("Deck already created", 405);
            
            var removeBrackets =
                cardUuids
                    .Remove(0, 1)
                    .Remove(cardUuids.Length - 2, 1)
                    .Replace("\"", string.Empty);

            var cardUuidsAsArray = removeBrackets.Split(", ");

            var data = new StringBuilder();

            if (cardUuidsAsArray.Length < 4) return ResponseTypes.BadRequest;
            var deck = DeckRepository.ConfigureDeck(token, cardUuidsAsArray);

            if (deck == null) return ResponseTypes.NotFoundRequest;
            deck.Cards.ForEach(card => data.Append(JsonSerializer.Serialize(card) + "," + Environment.NewLine));

            return ResponseTypes.CustomResponse(data.ToString(), 200, "application/json");
        }

        public static Response ConfigureDeck(string token, string cardUuids)
        {
            var removeBrackets =
                cardUuids
                    .Remove(0, 1)
                    .Remove(cardUuids.Length - 2, 1)
                    .Replace("\"", string.Empty);

            var cardUuidsAsArray = removeBrackets.Split(", ");

            var data = new StringBuilder();

            if (cardUuidsAsArray.Length < 4) return ResponseTypes.BadRequest;
            var deck = DeckRepository.ConfigureDeck(token, cardUuidsAsArray);

            if (deck == null) return ResponseTypes.NotFoundRequest;
            deck.Cards.ForEach(card => data.Append(JsonSerializer.Serialize(card) + "," + Environment.NewLine));

            return ResponseTypes.CustomResponse(data.ToString(), 200, "application/json");
        }
    }
}