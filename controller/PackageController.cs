using System;
using System.Text.Json;

namespace mtcg.controller
{
    public class PackageController
    {

        //TODO: Create Package - Create 4 Cards
        public static Response Post(string payload)
        {
            var response = new Response() {ContentType = "text/plain"};
            try
            {
                var json = JsonSerializer.Deserialize<Card[]>(payload);
                foreach (var card in json)
                {
                    Console.WriteLine($"Card-ID:{card.Uuid}");
                    Console.WriteLine($"Card-Type:{card.CardType}");
                    Console.WriteLine($"Card-Type:{card.Damage}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                return ResponseTypes.BadRequest;
            }
     
            return new Response();
        }
    }
}