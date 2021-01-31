using System;
using System.Collections.Generic;
using mtcg.classes.entities;
using mtcg.types;
using Npgsql;

namespace mtcg.repositories
{
    public static class CardRepository
    {

        public static IEnumerable<Card> SelectAll()
        {
            var retrievedCards = new List<Card>();
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.Credentials))
                {
                    using var query = new NpgsqlCommand("select * from card", connection);
                    connection.Open();

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
            }
            catch (PostgresException)
            {
                retrievedCards = new List<Card>();
            }

            return retrievedCards;
        }

        public static Card SelectCardByUuid(string uuid)
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
            catch (PostgresException pe)
            {
                Console.WriteLine(pe.Message);
                Console.WriteLine(pe.StackTrace);
                return null;
            }

            return card;
        }

        public static bool InsertCard(Card card)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.Credentials))
                {
                    connection.Open();
                    connection.TypeMapper.MapEnum<ElementType>("element_type");
                    using var query =
                        new NpgsqlCommand(
                            "insert into card(uuid, name, card_type, element_type, damage) values (@uuid,@name,@card_type, @element_type, @damage)",
                            connection);
                    query.Parameters.AddWithValue("uuid", Guid.Parse(card.Uuid));
                    query.Parameters.AddWithValue("name", card.Name);
                    query.Parameters.AddWithValue("card_type", card.CardType);
                    query.Parameters.AddWithValue("element_type", card.ElementType);
                    query.Parameters.AddWithValue("damage", card.Damage);
                    return (query.ExecuteNonQuery() > 0);
                }
            }
            catch (PostgresException)
            {
                return false;
            }
        }

        public static bool UpdateCard(Card card)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.Credentials))
                {
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
                    return (query.ExecuteNonQuery() > 0);
                }
            }
            catch (PostgresException)
            {
                return false;
            }
        }

        public static bool DeleteCard(string uuid)
        {
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            try
            {
                using var query = new NpgsqlCommand("delete from card where uuid::text = @uuid", connection);
                query.Parameters.AddWithValue("uuid", uuid);
                connection.Open();
                return query.ExecuteNonQuery() > 0;
            }
            catch (PostgresException)
            {
                return false;
            }
        }
    }
}