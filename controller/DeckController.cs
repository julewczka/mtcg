using System;
using System.Text;
using System.Text.Json;
using mtcg.classes.entities;
using mtcg.repositories;

namespace mtcg.controller
{
    public class DeckController
    {
        private readonly DeckRepository _deckRepo;

        public DeckController()
        {
            _deckRepo = new DeckRepository();
        }
        public Response GetDeckByUser(User user)
        {
            if (user?.Id == null) return RTypes.CError("user not found", 404);

            var deck = _deckRepo.GetDeckByUserUuid(user.Id);
            if (deck?.Uuid == null) return RTypes.CError("deck not found - create one", 404);

            deck.Cards = _deckRepo.GetCardsFromDeck(deck.Uuid);

            var content = new StringBuilder(JsonSerializer.Serialize(deck));
            return RTypes.CResponse(content.ToString(), 200, "application/json");
        }

        public Response CreateDeck(User user, string cardUuids)
        {
            if (user?.Id == null) return RTypes.Forbidden;
            if (GetDeckByUser(user).StatusCode == 200) return RTypes.CError("Deck already created", 405);
            
            var removeBrackets =
                cardUuids
                    .Remove(0, 1)
                    .Remove(cardUuids.Length - 2, 1)
                    .Replace("\"", string.Empty);

            var cardUuidsAsArray = removeBrackets.Split(", ");

            var data = new StringBuilder();

            if (cardUuidsAsArray.Length < 4) return RTypes.CError("Deck needs atleast 4 cards!", 400);
            var deck = _deckRepo.ConfigureDeck(user, cardUuidsAsArray);

            if (deck?.Uuid == null) return RTypes.NotFoundRequest;
            deck.Cards.ForEach(card => data.Append(JsonSerializer.Serialize(card) + "," + Environment.NewLine));

            return RTypes.CResponse(data.ToString(), 200, "application/json");
        }

        public Response ConfigureDeck(User user, string cardUuids)
        {
            var cardRepo = new CardRepository();
            
            var removeBrackets =
                cardUuids
                    .Remove(0, 1)
                    .Remove(cardUuids.Length - 2, 1)
                    .Replace("\"", string.Empty);

            var cardUuidsAsArray = removeBrackets.Split(", ");
            if (cardUuidsAsArray.Length < 4) return RTypes.CError("you must insert 4 id's", 400);

            foreach (var cardUuid in cardUuidsAsArray)
            {
                var card = cardRepo.GetByUuid(cardUuid);
                if (card?.Uuid == null) return RTypes.Forbidden;

                if (StackController.IsLocked(card)) return RTypes.CError($"{card.Uuid} is locked!", 403);
            }
            
            var deck = _deckRepo.ConfigureDeck(user, cardUuidsAsArray);
            return deck?.Uuid == null ? RTypes.NotFoundRequest : RTypes.Created;
        }
        
        //TODO: Add Delete method
    }
}