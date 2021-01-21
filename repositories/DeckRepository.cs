using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace mtcg.repositories
{
    public static class DeckRepository
    {
        private const string Credentials =
            "Server=127.0.0.1;Port=5432;Database=mtcg-db;User Id=mtcg-user;Password=mtcg-pw";

        public static Deck GetDeckByUserUuid(string userUuid)
        {
            var deck = new Deck();
            using var connection = new NpgsqlConnection(Credentials);
            using var query = new NpgsqlCommand("select * from deck where user_uuid::text = @user_uuid", connection);
            query.Parameters.AddWithValue("user_uuid", userUuid);
            connection.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    deck.Uuid = fetch["uuid"].ToString();
                }
            }
            catch (PostgresException pe)
            {
                Console.WriteLine(pe.Message);
                Console.WriteLine(pe.StackTrace);
                return null;
            }

            return deck;
        }

        public static Deck ConfigureDeck(string token, string[] cardUuids)
        {
            var cardList = new List<bool>();
            var user = UserRepository.SelectUserByToken(token);
            Console.WriteLine($"User-Id:{user.Id}");

            if (user == null) return null;
            user.Deck ??= GetDeckByUserUuid(user.Id);

            Console.WriteLine($"UserDeck-Id:{user.Deck.Uuid}");

            if (user.Deck.Uuid == null) user.Deck = AddDeck(user.Id);

            Console.WriteLine($"Card-Ids-Length:{cardUuids.Length}");

            if (cardUuids.Length <= 0) return null;

            var deckCards = GetCardsFromDeck(user.Deck.Uuid);

            if (user.Deck.Uuid == null) user.Deck = AddDeck(user.Id);

            Console.WriteLine($"deckCards-Length:{deckCards.Count}");

            if (deckCards.Count > 0)
            {
                if (!ClearDeckCards(user.Deck.Uuid)) return null;
                cardList.AddRange(cardUuids.Select(cardUuid => UpdateDeck(user.Deck.Uuid, cardUuid)));
            }
            else
            {
                cardList.AddRange(cardUuids.Select(cardUuid => AddRelationship(user.Deck.Uuid, cardUuid)));
            }
            
            Console.WriteLine($"CardList-Length:{cardList.Count}");

            if (cardList.Contains(false)) return null;

            user.Deck.Cards = GetCardsFromDeck(user.Deck.Uuid);

            return user.Deck;
        }

        private static bool AddRelationship(string deckUuid, string cardUuid)
        {
            if (string.IsNullOrEmpty(cardUuid)) return false;

            using var connection = new NpgsqlConnection(Credentials);
            using var query =
                new NpgsqlCommand("insert into deck_cards(deck_uuid, card_uuid) values (@deck_uuid, @card_uuid)",
                    connection);
            query.Parameters.AddWithValue("deck_uuid", Guid.Parse(deckUuid));
            query.Parameters.AddWithValue("card_uuid", Guid.Parse(cardUuid));
            connection.Open();
            try
            {
                return (query.ExecuteNonQuery() > 0);
            }
            catch (PostgresException)
            {
                return false;
            }
        }

        private static bool UpdateDeck(string deckUuid, string cardUuid)
        {
            return AddRelationship(deckUuid, cardUuid);
        }

        private static Deck AddDeck(string userUuid)
        {
            var deck = new Deck();
            using var connection = new NpgsqlConnection(Credentials);
            using var query = new NpgsqlCommand("insert into deck(user_uuid) values(@uuid) returning uuid", connection);
            query.Parameters.AddWithValue("uuid", Guid.Parse(userUuid));
            try
            {
                connection.Open();
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    deck.Uuid = fetch["uuid"].ToString();
                }
            }
            catch (PostgresException pe)
            {
                Console.WriteLine(pe.Message);
                Console.WriteLine(pe.StackTrace);

                return deck;
            }

            return deck;
        }

        private static bool ClearDeckCards(string deckUuid)
        {
            using var connection = new NpgsqlConnection(Credentials);
            using var query =
                new NpgsqlCommand("delete from deck_cards where deck_uuid::text = @deck_uuid", connection);
            query.Parameters.AddWithValue("deck_uuid", deckUuid);
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

        public static List<Card> GetCardsFromDeck(string deckUuid)
        {
            var cards = new List<Card>();
            using var connection = new NpgsqlConnection(Credentials);
            using var query =
                new NpgsqlCommand(
                    "select * from card join deck_cards dc on card.uuid = dc.card_uuid where deck_uuid::text = @deck_uuid",
                    connection);
            query.Parameters.AddWithValue("deck_uuid", deckUuid);
            connection.Open();
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
            catch (PostgresException pe)
            {
                Console.WriteLine(pe.Message);
                Console.WriteLine(pe.StackTrace);

                return null;
            }

            return cards;
        }
    }
}