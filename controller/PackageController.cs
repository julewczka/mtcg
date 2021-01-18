using System;
using System.Text.Json;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class PackageController
    {
        public static Response Post(string payload)
        {
            var cards = JsonSerializer.Deserialize<Card[]>(payload);
            return PackageRepository.CreatePackage(cards)
                ? new Response("Created") {ContentType = "text/plain", StatusCode = 201}
                : ResponseTypes.BadRequest;
        }
    }
}