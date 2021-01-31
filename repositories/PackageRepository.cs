using System;
using System.Collections.Generic;
using System.Linq;
using mtcg.classes.entities;
using mtcg.types;
using Npgsql;

namespace mtcg.repositories
{
    public class PackageRepository
    {
        private readonly NpgsqlConnection _connection;
        private readonly NpgsqlTransaction _transaction;

        public PackageRepository()
        {
            _connection = new NpgsqlConnection();
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        public Package SellPackage()
        {
            var packages = GetAllPackages();
            return packages.Count == 0 ? null : packages[(new Random()).Next(0, packages.Count)];
        }

        public List<Package> GetAllPackages()
        {
            var packages = new List<Package>();
            using var query = new NpgsqlCommand("select * from packages", _connection);
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }

            return packages;
        }

        public bool DeletePackage(string packUuid)
        {
            if (!DestroyPackRelation(packUuid)) return false;
            using var query = new NpgsqlCommand("delete from packages where uuid::text = @uuid", _connection);
            query.Parameters.AddWithValue("uuid", packUuid);
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

        public bool IsCardInPackages(string cardUuid)
        {
            using var query =
                new NpgsqlCommand(
                    "select * from pack_cards where card_uuid::text = @card_uuid",
                    _connection);
            query.Parameters.AddWithValue("card_uuid", cardUuid);
            try
            {
                return query.ExecuteScalar() != null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        private bool DestroyPackRelation(string packUuid)
        {
            using var query =
                new NpgsqlCommand("delete from pack_cards where pack_uuid::text = @pack_uuid", _connection);
            query.Parameters.AddWithValue("pack_uuid", packUuid);
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

        private List<Card> GetCardsFromPack(string uuid)
        {
            var cards = new List<Card>();
            using var query =
                new NpgsqlCommand(
                    "select * from card join pack_cards pc on card.uuid = pc.card_uuid where pack_uuid::text = @uuid",
                    _connection);
            query.Parameters.AddWithValue("uuid", uuid);
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }

            return cards;
        }

        public bool CreatePackage(Card[] cards)
        {
            var cardRepo = new CardRepository();
            var success = true;
            try
            {
                var transactionCards = cards.Select(cardRepo.AddCard).ToList();
                if (transactionCards.Contains(false)) success = false;

                var packUuid = AddPackage();
                if (string.IsNullOrEmpty(packUuid)) success = false;

                var transactionPackCards = cards.Select(card => AddRelationship(packUuid, card.Uuid)).ToList();
                if (transactionCards.Contains(false) || (transactionPackCards.Contains(false))) success = false;

                if (!success) _transaction.Rollback();
                _transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                _transaction.Rollback();
            }

            return success;
        }

        private string AddPackage()
        {
            var uuid = "";
            using var query = new NpgsqlCommand("insert into packages default values returning uuid", _connection,
                _transaction);
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
                return string.Empty;
            }

            return uuid;
        }

        private bool AddRelationship(string packUuid, string cardUuid)
        {
            if (string.IsNullOrEmpty(cardUuid)) return false;

            using var query =
                new NpgsqlCommand("insert into pack_cards(pack_uuid, card_uuid) values (@pack_uuid, @card_uuid)",
                    _connection, _transaction);
            query.Parameters.AddWithValue("pack_uuid", Guid.Parse(packUuid));
            query.Parameters.AddWithValue("card_uuid", Guid.Parse(cardUuid));
            try
            {
                return (query.ExecuteNonQuery() > 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }
    }
}