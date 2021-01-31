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
            var cardRepo = new CardRepository();
            
            var stack = SelectStackByUserId(uuid);

            if (stack?.Uuid == null) return null;

            var cardUuids = GetCardUuidsInStack(stack.Uuid);
            if (cardUuids == null) return null;


            cardUuids.ForEach(cardUuid =>
            {
                if (cardRepo.GetByUuid(cardUuid) != null)
                {
                    cards.Add(cardRepo.GetByUuid(cardUuid));
                }
            });

            return cards;
        }

        public static bool InsertStackCards(string userUuid, string cardUuid, NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            var userStack = SelectStackByUserId(userUuid);
            using var query =
                new NpgsqlCommand("insert into stack_cards(stack_uuid, card_uuid) values (@stack_uuid, @card_uuid)",
                    connection, transaction);
            query.Parameters.AddWithValue("stack_uuid", Guid.Parse(userStack.Uuid));
            query.Parameters.AddWithValue("card_uuid", Guid.Parse(cardUuid));

            return query.ExecuteNonQuery() > 0;
        }


        public static Response BuyPackage(string userUuid)
        {
            var userRepo = new UserRepository();
            var packRepo = new PackageRepository();
            
            var buyPack = packRepo.SellPackage();
            if (buyPack == null) return RTypes.CError("No package available at the moment!", 404);

            var user = userRepo.GetByUuid(userUuid);
            if (user.Coins < buyPack.Price) return RTypes.CError("Not enough coins!", 403);
            ;
            user.Coins -= buyPack.Price;

            var stack = SelectStackByUserId(userUuid);
            var stackUuid = stack.Uuid ?? CreateStack(userUuid);
            if (string.IsNullOrEmpty(stackUuid)) return RTypes.CError("Stack not found!", 404);

            var stackCards = buyPack.Cards.Select(card => AddRelationship(card.Uuid, stackUuid)).ToList();
            if (!packRepo.DeletePackage(buyPack.Uuid))
                return RTypes.CError("package couldn't be deleted!", 500);

            if (!userRepo.UpdateUser(user)) return RTypes.CError("User couldn't be updated!", 500);
            ;

            return stackCards.Contains(false)
                ? RTypes.CError("Buying package failed!", 500)
                : RTypes.HttpOk;
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

        public static Stack SelectStackByUserId(string uuid)
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

        public static bool DeleteCardFromStackByCardUuid(string stackUuid, string cardUuid, NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            using var query =
                new NpgsqlCommand(
                    "delete from stack_cards where stack_uuid::text = @stack_uuid and card_uuid::text = @card_uuid",
                    connection, transaction);
            query.Parameters.AddWithValue("stack_uuid", stackUuid);
            query.Parameters.AddWithValue("card_uuid", cardUuid);

            return query.ExecuteNonQuery() > 0;
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