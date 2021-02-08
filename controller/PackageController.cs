using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BIF.SWE1.Interfaces;
using mtcg.classes.entities;
using mtcg.repositories;

namespace mtcg.controller
{
    public class PackageController
    {
        private readonly PackageRepository _packRepo;

        public PackageController()
        {
            _packRepo = new PackageRepository();
        }
        public Response Get(IReadOnlyList<string> resource)
        {
            var fetchedPacks = new List<Package>();
            var data = new StringBuilder();

            switch (resource.Count)
            {
                case 1:
                    fetchedPacks.AddRange(_packRepo.GetAllPackages());
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
                    return RTypes.BadRequest;
            }

            return RTypes.CResponse(data.ToString(), 200, "application/json");
        }
        public Response Post(string token, string payload)
        {
            if (token != "admin-mtcgToken") return RTypes.Forbidden;

            var cards = JsonSerializer.Deserialize<Card[]>(payload);
            return _packRepo.CreatePackage(cards)
                ? RTypes.Created
                : RTypes.BadRequest;
        }
        
    }
}