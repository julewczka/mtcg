using System;
using System.Collections.Generic;
using System.Linq;
using mtcg.classes.entities;
using mtcg.types;
using Npgsql;

namespace mtcg.repositories
{
    public class DeckRepository
    {
        private readonly NpgsqlConnection _connection;
        private readonly NpgsqlTransaction _transaction;

        public DeckRepository()
        {
            _connection = new NpgsqlConnection();
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        /// <summary>
        /// Get deck by user uuid.
        /// Needs multiple connections because of parallel execution.
        /// </summary>
        /// <param name="userUuid">uuid of the user</param>
        /// <returns>returns a deck object</returns>
        public Deck GetDeckByUserUuid(string userUuid)
        {
            var deck = new Deck();
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }

            return deck;
        }

        public Deck ConfigureDeck(User user, IEnumerable<string> cardUuids)
        {
            var success = true;

            try
            {
                var cardList = new List<bool>();
                if (user?.Id == null) return null;
                user.Deck ??= GetDeckByUserUuid(user.Id);
                if (user.Deck.Uuid == null) user.Deck = AddDeck(user.Id);
                var deckCards = GetCardsFromDeck(user.Deck.Uuid);
                if (deckCards.Count > 0)
                {
                    if (!ClearDeckCards(user.Deck.Uuid)) success = false;
                    cardList.AddRange(cardUuids.Select(cardUuid =>
                    {
                        var isUpdated = UpdateDeck(user.Deck.Uuid, cardUuid);
                        if (!isUpdated) success = false;
                        return isUpdated;
                    }));
                }
                else
                {
                    cardList.AddRange(cardUuids.Select(cardUuid =>
                    {
                        var isAdded = AddRelationship(user.Deck.Uuid, cardUuid);
                        if (!isAdded) success = false;
                        return isAdded;
                    }));
                }

                if (cardList.Contains(false)) success = false;
                user.Deck.Cards = GetCardsFromDeck(user.Deck.Uuid);

                if (!success) _transaction.Rollback();

                _transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                _transaction.Rollback();
                return null;
            }
            
            return user.Deck;
        }

        private bool AddRelationship(string deckUuid, string cardUuid)
        {
            if (string.IsNullOrEmpty(cardUuid)) return false;
            
            using var query =
                new NpgsqlCommand("insert into deck_cards(deck_uuid, card_uuid) values (@deck_uuid, @card_uuid)",
                    _connection, _transaction);
            query.Parameters.AddWithValue("deck_uuid", Guid.Parse(deckUuid));
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

        private bool UpdateDeck(string deckUuid, string cardUuid)
        {
            return AddRelationship(deckUuid, cardUuid);
        }

        private Deck AddDeck(string userUuid)
        {
            var deck = new Deck();
            using var query =
                new NpgsqlCommand("insert into deck(user_uuid) values(@uuid) returning uuid", _connection,
                    _transaction);
            query.Parameters.AddWithValue("uuid", Guid.Parse(userUuid));
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    deck.Uuid = fetch["uuid"].ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }

            return deck;
        }

        private bool ClearDeckCards(string deckUuid)
        {
            using var query =
                new NpgsqlCommand("delete from deck_cards where deck_uuid::text = @deck_uuid", _connection,
                    _transaction);
            query.Parameters.AddWithValue("deck_uuid", deckUuid);
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

        public List<Card> GetCardsFromDeck(string deckUuid)
        {
            var cards = new List<Card>();
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                return null;
            }

            return cards;
        }

        public bool IsCardInDeck(string cardUuid)
        {
            using var query =
                new NpgsqlCommand("select * from deck_cards where card_uuid::text = @card_uuid", _connection);
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
    }
}