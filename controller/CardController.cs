using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using mtcg.classes.entities;
using mtcg.repositories;

namespace mtcg.controller
{
    public class CardController
    {
        private readonly CardRepository _cardRepo;
        private readonly StackRepository _stackRepo;

        public CardController()
        {
            _cardRepo = new CardRepository();
            _stackRepo = new StackRepository();
        }
        public Response Get(User user)
        {
            var fetchedCards = new List<Card>();
            var content = new StringBuilder();

            switch (user.Username)
            {
                case "admin":
                    fetchedCards.AddRange(_cardRepo.GetAllCards());
                    fetchedCards.ForEach(card => content.Append(JsonSerializer.Serialize(card) + "," + Environment.NewLine));
                    break;
                default:
                    var cards = _stackRepo.GetStack(user.Id);
                    content.Append(JsonSerializer.Serialize(cards) + "," + Environment.NewLine);
                    break;
            }

            return RTypes.CResponse(content.ToString(), 200, "application/json");
        }

        public Response Post(string payload)
        {
            var createCard = JsonSerializer.Deserialize<Card>(payload);
            if (createCard == null) return RTypes.BadRequest;

            return _cardRepo.AddCard(createCard)
                ? RTypes.Created
                : RTypes.BadRequest;
        }

        public Response Put(string uuid, string payload)
        {
            var updateCard = JsonSerializer.Deserialize<Card>(payload);
            if (updateCard == null) return RTypes.BadRequest;
            updateCard.Uuid = uuid;

            return _cardRepo.UpdateCard(updateCard)
                ? RTypes.Created
                : RTypes.BadRequest;
        }

        public  Response Delete(string uuid)
        {
            return _cardRepo.DeleteCard(uuid)
                ? RTypes.HttpOk
                : RTypes.BadRequest;
        }
    }
}