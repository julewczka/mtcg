using System;
using System.Text.Json;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class PackageController
    {
        public static Response Post(string payload)
        {
            var response = new Response() {ContentType = "text/plain"};
            try
            {
                var cards = JsonSerializer.Deserialize<Card[]>(payload);
                PackageRepository.CreatePackage(cards);
                
                Console.WriteLine("Does the Code reach here?");
                response.StatusCode = 201;
                response.SetContent("Created");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                return ResponseTypes.BadRequest;
            }
     
            return response;
        }
    }
}