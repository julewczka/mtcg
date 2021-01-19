using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Npgsql;

namespace mtcg.repositories
{
    public static class PackageRepository
    {
        private const string Credentials =
            "Server=127.0.0.1;Port=5432;Database=mtcg-db;User Id=mtcg-user;Password=mtcg-pw";

        //TODO: Do not add second Package and Pack_Card if Card already exists!
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
            using var query = new NpgsqlCommand("insert into pack_cards(pack_uuid, card_uuid) values (@pack_uuid, @card_uuid)", connection);
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