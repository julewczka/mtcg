using System;
using System.Collections.Generic;
using mtcg.classes.entities;
using mtcg.types;
using Npgsql;

namespace mtcg.repositories
{
    public static class TradingRepository
    {

        public static List<Trading> GetAllDeals()
        {
            var tradings = new List<Trading>();
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("select * from trading", connection);
            connection.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    var currentTrade = new Trading()
                    {
                        Uuid = fetch["uuid"].ToString(),
                        CardToTrade = fetch["card_uuid"].ToString(),
                        Trader = fetch["user_uuid"].ToString(),
                        CardType = fetch["card_type"].ToString(),
                        MinimumDamage = double.Parse(fetch["min_damage"].ToString())
                    };
                    tradings.Add(currentTrade);
                }
            }
            catch (PostgresException pe)
            {
                Console.WriteLine(pe.Message);
                Console.WriteLine(pe.StackTrace);
                return null;
            }

            return tradings;
        }

        public static Trading GetDealByUuid(string uuid)
        {
            var trading = new Trading();
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("select * from trading where uuid::text = @uuid", connection);
            query.Parameters.AddWithValue("uuid", uuid);
            connection.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    trading.Uuid = fetch["uuid"].ToString();
                    trading.CardToTrade = fetch["card_uuid"].ToString();
                    trading.Trader = fetch["user_uuid"].ToString();
                    trading.CardType = fetch["card_type"].ToString();
                    trading.MinimumDamage = double.Parse(fetch["min_damage"].ToString());
                }
            }
            catch (PostgresException pe)
            {
                Console.WriteLine(pe.Message);
                Console.WriteLine(pe.StackTrace);
            }

            return trading;
        }

        public static bool InsertTradingDeal(Trading trading)
        {
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query =
                new NpgsqlCommand(
                    "insert into trading(uuid, card_uuid, user_uuid, card_type, min_damage) values (@uuid, @card_uuid, @user_uuid, @card_type, @min_damage)",
                    connection);
            query.Parameters.AddWithValue("uuid", Guid.Parse(trading.Uuid));
            query.Parameters.AddWithValue("card_uuid", Guid.Parse(trading.CardToTrade));
            query.Parameters.AddWithValue("user_uuid", Guid.Parse(trading.Trader));
            query.Parameters.AddWithValue("card_type", trading.CardType);
            query.Parameters.AddWithValue("min_damage", trading.MinimumDamage);
            connection.Open();
            try
            {
                return query.ExecuteNonQuery() > 0;
            }
            catch (PostgresException pe)
            {
                Console.WriteLine(pe.Message);
                Console.WriteLine(pe.StackTrace);
                return false;
            }
        }
        public static bool StartToTrade(string tradingUuid, string cardUuid, string token)
        {
            //TODO: transactions verwenden
            var tradingTarget = GetDealByUuid(tradingUuid);
            var targetCard = CardRepository.SelectCardByUuid(tradingTarget.CardToTrade);
            var oldOwner = UserRepository.SelectUserByUuid(tradingTarget.Trader);
            var oldOwnerStack = StackRepository.GetStackByUserId(oldOwner.Id);
            var newOwner = UserRepository.SelectUserByToken(token);
            var newOwnerStack = StackRepository.GetStackByUserId(newOwner.Id);

            if (!SwitchCardOwner(newOwner.Id, targetCard.Uuid)) return false;
            if (!StackRepository.DeleteCardFromStackByCardUuid(oldOwnerStack.Uuid,targetCard.Uuid)) return false;
            if (!SwitchCardOwner(oldOwner.Id, cardUuid)) return false;
            if (!StackRepository.DeleteCardFromStackByCardUuid(newOwnerStack.Uuid,cardUuid)) return false;
            if (!DeleteTrading(tradingUuid)) return false;
            
            return true;
        }

        public static bool DeleteTrading(string tradingUuid)
        {
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query =
                new NpgsqlCommand("delete from trading where uuid::text = @uuid", connection);
            query.Parameters.AddWithValue("uuid", tradingUuid);

            connection.Open();
            try
            {
                return query.ExecuteNonQuery() > 0;
            }
            catch (PostgresException pe)
            {
                Console.WriteLine(pe.Message);
                Console.WriteLine(pe.StackTrace);
                return false;
            }
        }

        private static bool SwitchCardOwner(string userUuid, string cardUuid)
        {
            var userStack = StackRepository.GetStackByUserId(userUuid);
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query =
                new NpgsqlCommand("insert into stack_cards(stack_uuid, card_uuid) values (@stack_uuid, @card_uuid)",
                    connection);
            query.Parameters.AddWithValue("stack_uuid", Guid.Parse(userStack.Uuid));
            query.Parameters.AddWithValue("card_uuid", Guid.Parse(cardUuid));
            connection.Open();
            try
            {
                return query.ExecuteNonQuery() > 0;
            }
            catch (PostgresException pe)
            {
                Console.WriteLine(pe.Message);
                Console.WriteLine(pe.StackTrace);
                return false;
            }
        }
    }
}