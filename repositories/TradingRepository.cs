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
        private readonly NpgsqlConnection _connection;
        private readonly NpgsqlTransaction _transaction;
        private readonly StackRepository _stackRepo;
        private readonly CardRepository _cardRepo;
        private readonly UserRepository _userRepo;

        public TradingRepository()
        {
            _stackRepo = new StackRepository();
            _cardRepo = new CardRepository();
            _userRepo = new UserRepository();
            
            _connection = new NpgsqlConnection(ConnectionString.Credentials);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        public List<Trading> GetAllDeals()
        {
            var tradings = new List<Trading>();
            using var query = new NpgsqlCommand("select * from trading", _connection);
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

        /// <summary>
        /// Get the deal by its uuid.
        /// this method needs an own connection, because it is executed in parallel by multiple controllers.
        /// </summary>
        /// <param name="uuid">uuid of the trading deal</param>
        /// <returns></returns>
        public Trading GetDealByUuid(string uuid)
        {
            var trading = new Trading();
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("select * from trading where uuid::text = @uuid", connection);
            connection.Open();
            query.Parameters.AddWithValue("uuid", uuid);
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

        public bool AddDeal(Trading trading)
        {
            using var query =
                new NpgsqlCommand(
                    "insert into trading(uuid, card_uuid, user_uuid, card_type, min_damage) values (@uuid, @card_uuid, @user_uuid, @card_type, @min_damage)",
                    _connection);
            query.Parameters.AddWithValue("uuid", Guid.Parse(trading.Uuid));
            query.Parameters.AddWithValue("card_uuid", Guid.Parse(trading.CardToTrade));
            query.Parameters.AddWithValue("user_uuid", Guid.Parse(trading.Trader));
            query.Parameters.AddWithValue("card_type", trading.CardType);
            query.Parameters.AddWithValue("min_damage", trading.MinimumDamage);
            try
            {
                return query.ExecuteNonQuery() > 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        public bool DeleteDealByUuid(string tradingUuid)
        {
            using var query =
                new NpgsqlCommand("delete from trading where uuid::text = @uuid", _connection);
            query.Parameters.AddWithValue("uuid", tradingUuid);
            try
            {
                return query.ExecuteNonQuery() > 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// starts a transaction and performs a trading deal
        /// </summary>
        /// <param name="dealUuid">uuid of the trading deal</param>
        /// <param name="offeredCardUuid">uuid of the offered card</param>
        /// <param name="requester">user object of requester</param>
        /// <returns>true if transaction success</returns>
        public bool BeginTrade(string dealUuid, string offeredCardUuid, User requester)
        {

            var success = true;
            var deal = GetDealByUuid(dealUuid);
            var cardToTrade = _cardRepo.GetByUuid(deal.CardToTrade);
            var trader = _userRepo.GetByUuid(deal.Trader);
            var traderStack = _stackRepo.SelectStackByUserId(trader.Id);
            var requesterStack = _stackRepo.SelectStackByUserId(requester.Id);

            try
            {
                if (!SwitchCardOwner(requester.Id, cardToTrade.Uuid)) success = false;
                
                if (!_stackRepo.DeleteCardFromStackByCardUuid(traderStack.Uuid, cardToTrade.Uuid, _connection,
                    _transaction)) success = false;

                if (!SwitchCardOwner(trader.Id, offeredCardUuid)) success = false;
                
                if (!_stackRepo.DeleteCardFromStackByCardUuid(requesterStack.Uuid, offeredCardUuid, _connection,
                    _transaction)) success = false;

                if (!DeleteDeal(dealUuid)) success = false;
                
                if (!success)
                {
                    _transaction.Rollback();
                    return false;
                }

                _transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                _transaction.Rollback();
                return false;
            }

            return false;
        }

        private bool SwitchCardOwner(string userUuid, string cardUuid)
        {
            return _stackRepo.InsertStackCards(userUuid, cardUuid, _connection, _transaction);
        }

        private bool DeleteDeal(string tradingUuid)
        {
            using var query =
                new NpgsqlCommand("delete from trading where uuid::text = @uuid", _connection, _transaction);
            query.Parameters.AddWithValue("uuid", tradingUuid);
            return query.ExecuteNonQuery() > 0;
        }
    }
}