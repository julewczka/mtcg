using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using mtcg.classes.entities;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class PackageController
    {
        
        public static Response Get(IReadOnlyList<string> resource)
        {
            var fetchedPacks = new List<Package>();
            var data = new StringBuilder();

            switch (resource.Count)
            {
                case 1:
                    fetchedPacks.AddRange(PackageRepository.GetAllPackages());
                    fetchedPacks.ForEach(pack =>
                    {
                        var package = JsonSerializer.Serialize(pack);
                        data.Append(package + "," + Environment.NewLine);
                    });
                    break;
                case 2:
                //TODO: implement view for single package
                break;
                default:
                    return ResponseTypes.BadRequest;
            }

            return ResponseTypes.CustomResponse(data.ToString(), 200, "application/json");
        }
        public static Response Post(string token, string payload)
        {
            if (token != "admin-mtcgToken") return ResponseTypes.Forbidden;

            var cards = JsonSerializer.Deserialize<Card[]>(payload);
            return PackageRepository.CreatePackage(cards)
                ? ResponseTypes.Created
                : ResponseTypes.BadRequest;
        }
    }
}