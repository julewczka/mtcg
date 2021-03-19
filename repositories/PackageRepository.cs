using System;
using System.Collections.Generic;
using System.Linq;
using mtcg.classes.entities;
using mtcg.interfaces;
using mtcg.types;
using Npgsql;

namespace mtcg.repositories
{
    public class PackageRepository : IRepository<Package>
    {
        private readonly CardRepository _cardRepo;
        public PackageRepository()
        {
            _cardRepo = new CardRepository();
        }

        public Package SellPackage()
        {
            var packages = GetAll();
            return packages.Count == 0 ? null : packages[(new Random()).Next(0, packages.Count)];
        }

        public List<Package> GetAll()
        {
            var packages = new List<Package>();
            using var conn = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("select * from packages", conn);
            conn.Open();
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

        public bool Insert(Package pack)
        {
            using var conn = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("insert into packages (uuid, price) values (@uuid, @price)", conn);
            query.Parameters.AddWithValue("uuid", Guid.Parse(pack.Uuid));
            query.Parameters.AddWithValue("price", pack.Price);
            conn.Open();
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

        public bool Update(Package pack)
        {
            using var conn = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("update packages set price = @price where uuid::text = @uuid", conn);
            query.Parameters.AddWithValue("uuid", pack.Uuid);
            query.Parameters.AddWithValue("price", pack.Price);
            conn.Open();
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
        
        public bool Delete(Package pack)
        {
            using var conn = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("delete from packages where uuid::text = @uuid", conn);
            query.Parameters.AddWithValue("uuid", pack.Uuid);
            conn.Open();
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
        
        public bool DeletePackage(string packUuid, NpgsqlConnection conn, NpgsqlTransaction trans)
        {
            if (!DestroyPackRelation(packUuid, conn, trans)) return false;
            using var query = new NpgsqlCommand("delete from packages where uuid::text = @uuid", conn, trans);
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
            using var conn = new NpgsqlConnection(ConnectionString.Credentials);
            conn.Open();
            using var query =
                new NpgsqlCommand(
                    "select * from pack_cards where card_uuid::text = @card_uuid",
                    conn);
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

        private bool DestroyPackRelation(string packUuid, NpgsqlConnection conn, NpgsqlTransaction trans)
        {
            using var query =
                new NpgsqlCommand("delete from pack_cards where pack_uuid::text = @pack_uuid", conn, trans);
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
            using var conn = new NpgsqlConnection(ConnectionString.Credentials);
            conn.Open();
            using var query =
                new NpgsqlCommand(
                    "select * from card join pack_cards pc on card.uuid = pc.card_uuid where pack_uuid::text = @uuid",
                    conn);
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
            var success = true;
            using var conn = new NpgsqlConnection(ConnectionString.Credentials);
            conn.Open();
            var trans = conn.BeginTransaction();
            try
            {
                var transactionCards = cards.Select(card => _cardRepo.AddCard(card, conn, trans)).ToList();
                if (transactionCards.Contains(false)) success = false;

                var packUuid = AddPackage(conn, trans);
                if (string.IsNullOrEmpty(packUuid)) success = false;

                var transactionPackCards =
                    cards.Select(card => AddRelationship(packUuid, card.Uuid, conn, trans)).ToList();
                if (transactionCards.Contains(false) || (transactionPackCards.Contains(false))) success = false;

                if (!success) trans.Rollback();
                trans.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                trans.Rollback();
            }

            return success;
        }

        private string AddPackage(NpgsqlConnection conn, NpgsqlTransaction trans)
        {
            var uuid = "";
            using var query = new NpgsqlCommand("insert into packages default values returning uuid", conn,
                trans);
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

        private bool AddRelationship(string packUuid, string cardUuid, NpgsqlConnection conn, NpgsqlTransaction trans)
        {
            if (string.IsNullOrEmpty(cardUuid)) return false;

            using var query =
                new NpgsqlCommand("insert into pack_cards(pack_uuid, card_uuid) values (@pack_uuid, @card_uuid)",
                    conn, trans);
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