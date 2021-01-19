using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class CardController
    {
        public static Response Get(IReadOnlyList<string> resource)
        {
            var response = new Response() {ContentType = "application/json"};
            var fetchedCards = new List<Card>();
            var data = new StringBuilder();

            switch (resource.Count)
            {
                case 1:
                    fetchedCards.AddRange(CardRepository.SelectAll());
                    fetchedCards.ForEach(card =>
                    {
                        var fetchedCard = JsonSerializer.Serialize(card);
                        data.Append(fetchedCard + "," + Environment.NewLine);
                    });
                    response.StatusCode = 200;
                    response.SetContent(data.ToString());
                    break;
                case 2:
                    //resource[1] has to be the UUID of the card!
                    var fetchedSingleCard = CardRepository.SelectById(resource[1]);
                    if (fetchedSingleCard == null) return ResponseTypes.NotFoundRequest;
                    
                    data.Append(JsonSerializer.Serialize(fetchedSingleCard) + "," + Environment.NewLine);

                    response.StatusCode = 200;
                    response.SetContent(data.ToString());
                    break;
                default:
                    return ResponseTypes.BadRequest;
            }

            return response;
        }

        public static Response Post(string payload)
        {
            var createCard = JsonSerializer.Deserialize<Card>(payload);
            if (createCard == null) return ResponseTypes.BadRequest;

            return CardRepository.InsertCard(createCard)
                ? new Response("Created") {ContentType = "text/plain", StatusCode = 201}
                : ResponseTypes.BadRequest;
        }
        
        public static Response Put(string uuid, string payload)
        {
            var updateCard = JsonSerializer.Deserialize<Card>(payload);
            if (updateCard == null) return ResponseTypes.BadRequest;
            updateCard.Uuid = uuid;

            return CardRepository.UpdateCard(updateCard)
                ? new Response("Updated") {ContentType = "text/plain", StatusCode = 201}
                : ResponseTypes.BadRequest;
        }

        public static Response Delete(string uuid)
        {
            return CardRepository.DeleteCard(uuid)
                ? new Response("OK") {ContentType = "text/plain", StatusCode = 200}
                : ResponseTypes.BadRequest;
        }
    }
}