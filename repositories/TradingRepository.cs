using System;
using System.Collections.Generic;
using System.Text;
using mtcg.classes.entities;
using mtcg.controller;
using mtcg.types;
using Npgsql;

namespace mtcg.repositories
{
    public class TradingRepository
    {
        private static NpgsqlConnection connection;
        private static NpgsqlTransaction transaction;

        public TradingRepository()
        {
            connection = new NpgsqlConnection(ConnectionString.Credentials);
            connection.Open();
            transaction = connection.BeginTransaction();
        }
        public List<Trading> GetAllDeals()
        {
            var tradings = new List<Trading>();
            //using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("select * from trading", connection);
            //connection.Open();
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

        public Trading GetDealByUuid(string uuid)
        {
            var trading = new Trading();
            //using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("select * from trading where uuid::text = @uuid", connection);
            query.Parameters.AddWithValue("uuid", uuid);
            //connection.Open();
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

        public static bool DeleteDealByUuid(string tradingUuid)
        {
            using var conn = new NpgsqlConnection(ConnectionString.Credentials);
            using var query =
                new NpgsqlCommand("delete from trading where uuid::text = @uuid", conn);
            query.Parameters.AddWithValue("uuid", tradingUuid);
            conn.Open();
            return query.ExecuteNonQuery() > 0;
        }

        public Response BeginTradeTransaction(string dealUuid, string offeredCardUuid, string token)
        {
            var success = true;
            var content = new StringBuilder();

            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            connection.Open();
            var transaction = connection.BeginTransaction();

            var deal = GetDealByUuid(dealUuid);
            var cardToTrade = CardRepository.SelectCardByUuid(deal.CardToTrade);
            var trader = UserRepository.SelectUserByUuid(deal.Trader);
            var traderStack = StackRepository.GetStackByUserId(trader.Id);
            var requester = UserRepository.SelectUserByToken(token);
            var requesterStack = StackRepository.GetStackByUserId(requester.Id);

            try
            {
                if (!SwitchCardOwner(requester.Id, cardToTrade.Uuid, connection, transaction))
                {
                    content.Append("switch requested card to trade requester failed!");
                    success = false;
                }

                if (!StackRepository.DeleteCardFromStackByCardUuid(traderStack.Uuid, cardToTrade.Uuid, connection,
                    transaction))
                {
                    content.Append("remove card from traders stack failed!");
                    success = false;
                }

                if (!SwitchCardOwner(trader.Id, offeredCardUuid, connection, transaction))
                { 
                    content.Append("switch offered card to trader failed!");
                    success = false;
                }

                if (!StackRepository.DeleteCardFromStackByCardUuid(requesterStack.Uuid, offeredCardUuid, connection,
                    transaction))
                {
                    content.Append("remove card from requesters stack failed!");
                    success = false;
                }

                if (!DeleteTrading(dealUuid, connection, transaction))
                {
                    content.Append("delete deal failed!");
                    success = false;
                }

                if (!success)
                {
                    transaction.Rollback();
                    return ResponseTypes.CustomError(content.ToString(), 400);
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                transaction.Rollback();
                return ResponseTypes.BadRequest;
            }

            return ResponseTypes.Created;
        }

        private static bool SwitchCardOwner(string userUuid, string cardUuid, NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            return StackRepository.InsertStackCards(userUuid, cardUuid, connection, transaction);
        }
        private static bool DeleteTrading(string tradingUuid, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            using var query =
                new NpgsqlCommand("delete from trading where uuid::text = @uuid", connection, transaction);
            query.Parameters.AddWithValue("uuid", tradingUuid);
            return query.ExecuteNonQuery() > 0;
        }

    }
}