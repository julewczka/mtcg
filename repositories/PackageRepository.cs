using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace mtcg.repositories
{
    public static class PackageRepository
    {
        private const string Credentials =
            "Server=127.0.0.1;Port=5432;Database=mtcg-db;User Id=mtcg-user;Password=mtcg-pw";

        public static Package SellPackage()
        {
            var packages = GetAllPackages();
            return packages.Count == 0 ? null : packages[(new Random()).Next(0, packages.Count)];
        }
        public static List<Package> GetAllPackages()
        {
            var packages = new List<Package>();
            using var connection = new NpgsqlConnection(Credentials);
            using var query = new NpgsqlCommand("select * from packages", connection);
            connection.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    var package = new Package()
                    {
                        Uuid = fetch["uuid"].ToString(),
                        Cards = GetCardsFromPack(fetch["Uuid"].ToString()),
                        Price = double.Parse(fetch["price"].ToString())
                    };
                    packages.Add(package);
                }
            }
            catch (PostgresException)
            {
                return null;
            }
            return packages;
        }

        public static bool DeletePackage(string packUuid)
        {
            if (!DestroyPackRelation(packUuid)) return false;
            using var connection = new NpgsqlConnection(Credentials);
            using var query = new NpgsqlCommand("delete from packages where uuid::text = @uuid",connection);
            query.Parameters.AddWithValue("uuid", packUuid);
            connection.Open();
            try
            {
                return query.ExecuteNonQuery() > 0;
            }
            catch (PostgresException)
            {
                return false;
            }
        }

        private static bool DestroyPackRelation(string packUuid)
        {
            using var connection = new NpgsqlConnection(Credentials);
            using var query = new NpgsqlCommand("delete from pack_cards where pack_uuid::text = @pack_uuid",connection);
            query.Parameters.AddWithValue("pack_uuid", packUuid);
            connection.Open();
            try
            {
                return query.ExecuteNonQuery() > 0;
            }
            catch (PostgresException)
            {
                return false;
            }
        }

        private static List<Card> GetCardsFromPack(string uuid)
        {
            var cards = new List<Card>();
            using var connection = new NpgsqlConnection(Credentials);
            using var query =
                new NpgsqlCommand(
                    "select * from card join pack_cards pc on card.uuid = pc.card_uuid where pack_uuid::text = @uuid",
                    connection);
            query.Parameters.AddWithValue("uuid", uuid);
            connection.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    var currentCard = new Card()
                    {
                        Uuid = fetch["uuid"].ToString(),
                        Name = fetch["name"].ToString(),
                        CardType = fetch["card_type"].ToString(),
                        ElementType = Card.GetElementType(fetch["element_type"].ToString()),
                        Damage = double.Parse(fetch["damage"].ToString())
                    };
                    cards.Add(currentCard);
                }
            }
            catch (PostgresException)
            {
                return null;
            }
            
            return cards;
        }
        
        public static bool CreatePackage(Card[] cards)
        {
            var transactionCards = cards.Select(CardRepository.InsertCard).ToList();
            if (transactionCards.Contains(false)) return false;

            var packUuid = AddPackage();
            if (string.IsNullOrEmpty(packUuid)) return false;

            var transactionPackCards = cards.Select(card => AddRelationship(packUuid, card.Uuid)).ToList();

            return (!transactionCards.Contains(false) && !(transactionPackCards.Contains(false)));
        }

        private static string AddPackage()
        {
            var uuid = "";
            using var connection = new NpgsqlConnection(Credentials);
            using var query = new NpgsqlCommand("insert into packages default values returning uuid", connection);
            try
            {
                connection.Open();
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    uuid = fetch["uuid"].ToString();
                }
            }
            catch (PostgresException)
            {
                return "";
            }

            return uuid;
        }

        private static bool AddRelationship(string packUuid, string cardUuid)
        {
            if (string.IsNullOrEmpty(cardUuid)) return false;

            using var connection = new NpgsqlConnection(Credentials);
            using var query =
                new NpgsqlCommand("insert into pack_cards(pack_uuid, card_uuid) values (@pack_uuid, @card_uuid)",
                    connection);
            query.Parameters.AddWithValue("pack_uuid", Guid.Parse(packUuid));
            query.Parameters.AddWithValue("card_uuid", Guid.Parse(cardUuid));
            connection.Open();
            try
            {
                return (query.ExecuteNonQuery() > 0);
            }
            catch (PostgresException)
            {
                return false;
            }
        }
    }
}