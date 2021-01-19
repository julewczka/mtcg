using System;
using System.Collections.Generic;
using Npgsql;

namespace mtcg.repositories
{
    public static class StackRepository
    {
        private const string Credentials =
            "Server=127.0.0.1;Port=5432;Database=mtcg-db;User Id=mtcg-user;Password=mtcg-pw";

        private static Stack GetStackByUserId(string uuid)
        {
            var stack = new Stack();
            using var connection = new NpgsqlConnection(Credentials);
            using var query = new NpgsqlCommand("select * from stack where user_uuid::text = @uuid",connection);
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
            using var query = new NpgsqlCommand("select * from stack_cards where stack_uuid::text = @stack_uuid",connection);
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
        public static List<Card> GetStack(string uuid)
        {
            var cards = new List<Card>();
            
            var stack = GetStackByUserId(uuid);
            if (stack == null) return null;

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
        
    }
}