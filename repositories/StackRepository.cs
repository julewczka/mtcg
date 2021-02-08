using System;
using System.Collections.Generic;
using System.Linq;
using mtcg.classes.entities;
using mtcg.types;
using Npgsql;

namespace mtcg.repositories
{
    public class StackRepository
    {
        /// <summary>
        /// Show all acquired cards
        /// </summary>
        /// <param name="uuid"> uuid of the user</param>
        /// <returns>returns a list of cards</returns>
        public List<Card> GetStack(string uuid)
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

        public bool InsertStackCards(string userUuid, string cardUuid, NpgsqlConnection connection,
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


        public bool BuyPackage(string userUuid)
        {
            var success = true;
            var userRepo = new UserRepository();
            var packRepo = new PackageRepository();
            using var conn = new NpgsqlConnection(ConnectionString.Credentials);
            conn.Open();
            var trans = conn.BeginTransaction();
   
            try
            {

                var buyPack = packRepo.SellPackage();
                if (buyPack?.Uuid == null) success = false;

                var user = userRepo.GetByUuid(userUuid);
                if (user?.Id == null || user.Coins < buyPack.Price) success = false;

                user.Coins -= buyPack.Price;

                var stack = SelectStackByUserId(userUuid);
                var stackUuid = stack.Uuid ?? CreateStack(userUuid, conn, trans);
                if (string.IsNullOrEmpty(stackUuid)) success = false;

                var stackCards = buyPack.Cards.Select(card => AddRelationship(card.Uuid, stackUuid, conn, trans))
                    .ToList();
                if (stackCards.Contains(false)) success = false;
                if (!packRepo.DeletePackage(buyPack.Uuid, conn, trans)) success = false;
                if (!userRepo.UpdateUserForTransaction(user, conn, trans)) success = false;

                if (!success)
                {
                    trans.Rollback();
                    return false;
                }

                trans.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                success = false;
                trans.Rollback();
            }


            return success;
        }

        private bool AddRelationship(string cardUuid, string stackUuid, NpgsqlConnection conn,
            NpgsqlTransaction trans)
        {
            if (string.IsNullOrEmpty(stackUuid)) return false;
            using var query =
                new NpgsqlCommand("insert into stack_cards(stack_uuid, card_uuid) values(@stack_uuid, @card_uuid)",
                    conn, trans);
            query.Parameters.AddWithValue("stack_uuid", Guid.Parse(stackUuid));
            query.Parameters.AddWithValue("card_uuid", Guid.Parse(cardUuid));
            try
            {
                return query.ExecuteNonQuery() > 0;
            }
            catch (PostgresException)
            {
                return false;
            }
        }

        private string CreateStack(string userUuid, NpgsqlConnection conn, NpgsqlTransaction trans)
        {
            var uuid = "";
            using var query =
                new NpgsqlCommand("insert into stack(user_uuid) values(@uuid) returning uuid", conn, trans);
            query.Parameters.AddWithValue("uuid", Guid.Parse(userUuid));
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    uuid = fetch["uuid"].ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }

            return uuid;
        }

        public Stack SelectStackByUserId(string uuid)
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

        public bool DeleteCardFromStackByCardUuid(string stackUuid, string cardUuid, NpgsqlConnection connection,
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

        public bool IsCardInStack(string cardUuid, string stackUuid)
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

        private List<string> GetCardUuidsInStack(string stackUuid)
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