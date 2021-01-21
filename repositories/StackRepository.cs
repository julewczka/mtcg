using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace mtcg.repositories
{
    public static class StackRepository
    {
        private const string Credentials =
            "Server=127.0.0.1;Port=5432;Database=mtcg-db;User Id=mtcg-user;Password=mtcg-pw";
        
        public static List<Card> GetStack(string uuid)
        {
            var cards = new List<Card>();

            var stack = GetStackByUserId(uuid);

            if (stack?.Id == null) return null;

            var cardUuids = GetCardUuidsInStack(stack.Id);
            if (cardUuids == null) return null;


            cardUuids.ForEach(cardUuid =>
            {
                if (CardRepository.SelectById(cardUuid) != null)
                {
                    cards.Add(CardRepository.SelectById(cardUuid));
                }
            });

            return cards;
        }

        public static bool BuyPackage(string userUuid)
        {
            var buyPack = PackageRepository.SellPackage();
            if (buyPack == null) return false;

            var stack = GetStackByUserId(userUuid);

            var stackUuid = stack.Id ?? CreateStack(userUuid);

            if (string.IsNullOrEmpty(stackUuid)) return false;
            
            var stackCards = buyPack.Cards.Select(card => AddRelationship(card.Uuid, stackUuid)).ToList();
            if (!PackageRepository.DeletePackage(buyPack.Uuid)) return false;
            return !stackCards.Contains(false);
        }

        private static bool AddRelationship(string cardUuid, string stackUuid)
        {
            if (string.IsNullOrEmpty(stackUuid)) return false;

            using var connection = new NpgsqlConnection(Credentials);
            using var query =
                new NpgsqlCommand("insert into stack_cards(stack_uuid, card_uuid) values(@stack_uuid, @card_uuid)",
                    connection);
            query.Parameters.AddWithValue("stack_uuid", Guid.Parse(stackUuid));
            query.Parameters.AddWithValue("card_uuid", Guid.Parse(cardUuid));
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

        private static string CreateStack(string userUuid)
        {
            var uuid = "";
            using var connection = new NpgsqlConnection(Credentials);
            using var query =
                new NpgsqlCommand("insert into stack(user_uuid) values(@uuid) returning uuid", connection);
            query.Parameters.AddWithValue("uuid", Guid.Parse(userUuid));
            connection.Open();
            try
            {
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

        private static Stack GetStackByUserId(string uuid)
        {
            var stack = new Stack();
            using var connection = new NpgsqlConnection(Credentials);
            using var query = new NpgsqlCommand("select * from stack where user_uuid::text = @uuid", connection);
            query.Parameters.AddWithValue("uuid", uuid);
            connection.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    stack.Id = fetch["uuid"].ToString();
                }
            }
            catch (PostgresException)
            {
                return null;
            }

            return stack;
        }

        private static List<string> GetCardUuidsInStack(string stackUuid)
        {
            var cardUuids = new List<string>();
            using var connection = new NpgsqlConnection(Credentials);
            using var query = new NpgsqlCommand("select * from stack_cards where stack_uuid::text = @stack_uuid",
                connection);
            query.Parameters.AddWithValue("stack_uuid", stackUuid);
            connection.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    cardUuids.Add(fetch["card_uuid"].ToString());
                }
            }
            catch (PostgresException)
            {
                return null;
            }

            return cardUuids;
        }
    }
}