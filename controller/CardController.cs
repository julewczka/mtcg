using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using mtcg.repositories;

namespace mtcg.controller
{
    public class CardController
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
                default:
                    return ResponseTypes.BadRequest;
            }

            return response;
        }
    }
}