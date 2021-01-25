using System;
using System.Collections.Generic;
using System.Linq;
using mtcg.classes.entities;
using mtcg.controller;
using mtcg.types;
using Npgsql;

namespace mtcg.repositories
{
    public static class StackRepository
    {
        /// <summary>
        /// Show all acquired cards
        /// </summary>
        /// <param name="uuid"> uuid of the user</param>
        /// <returns>returns a list of cards</returns>
        public static List<Card> GetStack(string uuid)
        {
            var cards = new List<Card>();

            var stack = GetStackByUserId(uuid);

            if (stack?.Uuid == null) return null;

            var cardUuids = GetCardUuidsInStack(stack.Uuid);
            if (cardUuids == null) return null;


            cardUuids.ForEach(cardUuid =>
            {
                if (CardRepository.SelectCardByUuid(cardUuid) != null)
                {
                    cards.Add(CardRepository.SelectCardByUuid(cardUuid));
                }
            });

            return cards;
        }

        public static Response BuyPackage(string userUuid)
        {
            var buyPack = PackageRepository.SellPackage();
            if (buyPack == null) return ResponseTypes.CustomError("No package available at the moment!", 404);

            var user = UserRepository.SelectUserByUuid(userUuid);
            if (user.Coins < buyPack.Price) return ResponseTypes.CustomError("Not enough coins!", 403);
            ;
            user.Coins -= buyPack.Price;

            var stack = GetStackByUserId(userUuid);
            var stackUuid = stack.Uuid ?? CreateStack(userUuid);
            if (string.IsNullOrEmpty(stackUuid)) return ResponseTypes.CustomError("Stack not found!", 404);

            var stackCards = buyPack.Cards.Select(card => AddRelationship(card.Uuid, stackUuid)).ToList();
            if (!PackageRepository.DeletePackage(buyPack.Uuid))
                return ResponseTypes.CustomError("package couldn't be deleted!", 500);

            if (!UserRepository.UpdateUser(user)) return ResponseTypes.CustomError("User couldn't be updated!", 500);
            ;

            return stackCards.Contains(false)
                ? ResponseTypes.CustomError("Buying package failed!", 500)
                : ResponseTypes.HttpOk;
        }

        private static bool AddRelationship(string cardUuid, string stackUuid)
        {
            if (string.IsNullOrEmpty(stackUuid)) return false;

            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
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
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
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

        public static Stack GetStackByUserId(string uuid)
        {
            var stack = new Stack();
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("select * from stack where user_uuid::text = @uuid", connection);
            query.Parameters.AddWithValue("uuid", uuid);
            connection.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    stack.Uuid = fetch["uuid"].ToString();
                }
            }
            catch (PostgresException)
            {
                return null;
            }

            return stack;
        }

        public static bool DeleteCardFromStackByCardUuid(string stackUuid, string cardUuid)
        {
            //TODO: connection nach außen schieben
            //commit & rollback
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            //var transaction = connection.BeginTransaction();
            using var query =
                new NpgsqlCommand("delete from stack_cards where stack_uuid::text = @stack_uuid and card_uuid::text = @card_uuid", connection);
            query.Parameters.AddWithValue("stack_uuid", stackUuid);
            query.Parameters.AddWithValue("card_uuid", cardUuid);
            connection.Open();
            try
            {
                var result = query.ExecuteNonQuery() > 0;
                connection.Close();
                return result;
            }
            catch (Exception pe)
            {
                Console.WriteLine(pe.Message);
                Console.WriteLine(pe.StackTrace);
                return false;
            }
        }

        public static bool IsCardInStack(string cardUuid, string stackUuid)
        {
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query =
                new NpgsqlCommand(
                    "select * from stack_cards where stack_uuid::text = @stack_uuid and card_uuid::text = @card_uuid",
                    connection);
            query.Parameters.AddWithValue("stack_uuid", stackUuid);
            query.Parameters.AddWithValue("card_uuid", cardUuid);
            connection.Open();
            try
            {
                return query.ExecuteScalar() != null;
            }
            catch (PostgresException pe)
            {
                Console.WriteLine(pe.Message);
                Console.WriteLine(pe.StackTrace);
                return false;
            }
        }

        private static List<string> GetCardUuidsInStack(string stackUuid)
        {
            var cardUuids = new List<string>();
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
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