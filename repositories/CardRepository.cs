using System;
using System.Collections.Generic;
using mtcg.classes.entities;
using mtcg.interfaces;
using mtcg.types;
using Npgsql;

namespace mtcg.repositories
{
    public class CardRepository : IRepository<Card>
    {

        public List<Card> GetAll()
        {
            var retrievedCards = new List<Card>();
            using var conn = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("select * from card", conn);
            conn.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    var currentCard = new Card
                    {
                        Uuid = fetch["uuid"].ToString(),
                        Name = fetch["name"].ToString(),
                        ElementType = Card.GetElementType(fetch["element_type"].ToString()),
                        CardType = fetch["card_type"].ToString(),
                        Damage = (double) fetch["damage"]
                    };
                    retrievedCards.Add(currentCard);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }

            return retrievedCards;
        }

        /// <summary>
        /// Get card by card-uuid.
        /// Needs multiple connections because it runs parallel.
        /// </summary>
        /// <param name="uuid">uuid of the card</param>
        /// <returns>returns requested card</returns>
        public Card GetByUuid(string uuid)
        {
            var card = new Card();
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("select * from card where uuid::text = @uuid", connection);
            query.Parameters.AddWithValue("uuid", uuid);
            connection.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    card.Uuid = fetch["uuid"].ToString();
                    card.Name = fetch["name"].ToString();
                    card.ElementType = Card.GetElementType(fetch["element_type"].ToString());
                    card.CardType = fetch["card_type"].ToString();
                    card.Damage = (double) fetch["damage"];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }

            return card;
        }

        public bool AddCard(Card card, NpgsqlConnection conn = null, NpgsqlTransaction trans = null)
        {
            using var connection = conn ?? new NpgsqlConnection(ConnectionString.Credentials);
            if(conn == null) connection.Open();
            connection.TypeMapper.MapEnum<ElementType>("element_type");
            using var query =
                new NpgsqlCommand(
                    "insert into card(uuid, name, card_type, element_type, damage) values (@uuid,@name,@card_type, @element_type, @damage)",
                    connection, trans);
            query.Parameters.AddWithValue("uuid", Guid.Parse(card.Uuid));
            query.Parameters.AddWithValue("name", card.Name);
            query.Parameters.AddWithValue("card_type", card.CardType);
            query.Parameters.AddWithValue("element_type", card.ElementType);
            query.Parameters.AddWithValue("damage", card.Damage);
            try
            {
                return (query.ExecuteNonQuery() > 0);
            }
            catch (PostgresException)
            {
                return false;
            }
        }

        public bool Insert(Card card)
        {
            using var conn = new NpgsqlConnection(ConnectionString.Credentials);
            conn.TypeMapper.MapEnum<ElementType>("element_type");
            using var query =
                new NpgsqlCommand(
                    "insert into card(uuid, name, card_type, element_type, damage) values (@uuid,@name,@card_type, @element_type, @damage)",
                    conn);
            query.Parameters.AddWithValue("uuid", Guid.Parse(card.Uuid));
            query.Parameters.AddWithValue("name", card.Name);
            query.Parameters.AddWithValue("card_type", card.CardType);
            query.Parameters.AddWithValue("element_type", card.ElementType);
            query.Parameters.AddWithValue("damage", card.Damage);
            conn.Open();
            try
            {
                return (query.ExecuteNonQuery() > 0);
            }
            catch (PostgresException)
            {
                return false;
            }
        }
        public bool Update(Card card)
        {
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            connection.Open();
            connection.TypeMapper.MapEnum<ElementType>("element_type");
            using var query =
                new NpgsqlCommand(
                    "update card set name = @name, card_type = @card_type, element_type = @element_type, damage = @damage where uuid::text = @uuid",
                    connection);
            query.Parameters.AddWithValue("uuid", card.Uuid);
            query.Parameters.AddWithValue("name", card.Name);
            query.Parameters.AddWithValue("card_type", card.CardType);
            query.Parameters.AddWithValue("element_type", card.ElementType);
            query.Parameters.AddWithValue("damage", card.Damage);
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

        public bool Delete(Card card)
        {
            using var conn = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("delete from card where uuid::text = @uuid", conn);
            query.Parameters.AddWithValue("uuid", card.Uuid);
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
    }
}