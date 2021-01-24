using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using mtcg.classes.entities;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class CardController
    {
        public static Response Get(string token)
        {
            var fetchedCards = new List<Card>();
            var content = new StringBuilder();

            switch (token)
            {
                case "admin-mtcgToken":
                    fetchedCards.AddRange(CardRepository.SelectAll());
                    fetchedCards.ForEach(card => content.Append(JsonSerializer.Serialize(card) + "," + Environment.NewLine));
                    break;
                default:
                    var user = UserRepository.SelectUserByToken(token);
                    var cards = StackRepository.GetStack(user.Id);
                    content.Append(JsonSerializer.Serialize(cards) + "," + Environment.NewLine);
                    break;
            }

            return ResponseTypes.CustomResponse(content.ToString(), 200, "application/json");
        }

        public static Response Post(string payload)
        {
            var createCard = JsonSerializer.Deserialize<Card>(payload);
            if (createCard == null) return ResponseTypes.BadRequest;

            return CardRepository.InsertCard(createCard)
                ? ResponseTypes.Created
                : ResponseTypes.BadRequest;
        }

        public static Response Put(string uuid, string payload)
        {
            var updateCard = JsonSerializer.Deserialize<Card>(payload);
            if (updateCard == null) return ResponseTypes.BadRequest;
            updateCard.Uuid = uuid;

            return CardRepository.UpdateCard(updateCard)
                ? ResponseTypes.Created
                : ResponseTypes.BadRequest;
        }

        public static Response Delete(string uuid)
        {
            return CardRepository.DeleteCard(uuid)
                ? ResponseTypes.HttpOk
                : ResponseTypes.BadRequest;
        }
    }
}