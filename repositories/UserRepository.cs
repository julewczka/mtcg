using System;
using System.Collections.Generic;
using mtcg.classes.entities;
using mtcg.types;
using Npgsql;

namespace mtcg.repositories
{
    public class UserRepository
    {
        private readonly NpgsqlConnection _connection;

        public UserRepository()
        {
            _connection = new NpgsqlConnection(ConnectionString.Credentials);
            _connection.Open();
        }

        public IEnumerable<User> GetAllUsers()
        {
            var deckRepo = new DeckRepository();
            var retrievedUsers = new List<User>();
            using var query = new NpgsqlCommand("select * from \"user\"", _connection);

            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    var currentUser = new User
                    {
                        Id = fetch["uuid"].ToString(),
                        Username = fetch["username"].ToString(),
                        Name = fetch["name"].ToString(),
                        Token = fetch["token"].ToString(),
                        Bio = fetch["bio"].ToString(),
                        Image = fetch["image"].ToString(),
                        Coins = double.Parse(fetch["coins"].ToString()),
                    };

                    var deck = deckRepo.GetDeckByUserUuid(currentUser.Id);
                    if (deck?.Uuid != null)
                    {
                        var deckCards = deckRepo.GetCardsFromDeck(deck.Uuid);
                        currentUser.Deck.Uuid = deck.Uuid;
                        currentUser.Deck.Cards = deckCards;
                    }

                    retrievedUsers.Add(currentUser);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }

            return retrievedUsers;
        }

        /**
         * Get a single user
         * returns an user object
         */
        public User GetByUsername(string username)
        {
            var user = new User();
            try
            {
                using var connection = new NpgsqlConnection(ConnectionString.Credentials);
                using var query = new NpgsqlCommand("select * from \"user\" where username = @username", connection);
                query.Parameters.AddWithValue("username", username);
                connection.Open();
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    user.Id = fetch["uuid"].ToString();
                    user.Username = fetch["username"].ToString();
                    user.Name = fetch["name"].ToString();
                    user.Token = fetch["token"].ToString();
                    user.Bio = fetch["bio"].ToString();
                    user.Image = fetch["image"].ToString();
                    user.Coins = string.IsNullOrEmpty(fetch["coins"].ToString())
                        ? 20
                        : double.Parse(fetch["coins"].ToString());
                }
            }

            catch (PostgresException)
            {
                return null;
            }

            return user;
        }

        public User GetByUuid(string uuid)
        {
            var user = new User();

            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("select * from \"user\" where uuid::text = @uuid", connection);
            query.Parameters.AddWithValue("uuid", uuid);
            connection.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    user.Id = fetch["uuid"].ToString();
                    user.Username = fetch["username"].ToString();
                    user.Name = fetch["name"].ToString();
                    user.Token = fetch["token"].ToString();
                    user.Bio = fetch["bio"].ToString();
                    user.Image = fetch["image"].ToString();
                    user.Coins = string.IsNullOrEmpty(fetch["coins"].ToString())
                        ? 20
                        : int.Parse(fetch["coins"].ToString());
                }
            }

            catch (PostgresException)
            {
                return null;
            }

            return user;
        }

        public User GetByToken(string token)
        {
            var user = new User();

            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("select * from \"user\" where token = @token", connection);
            query.Parameters.AddWithValue("token", token);
            connection.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    user.Id = fetch["uuid"].ToString();
                    user.Username = fetch["username"].ToString();
                    user.Name = fetch["name"].ToString();
                    user.Token = fetch["token"].ToString();
                    user.Bio = fetch["bio"].ToString();
                    user.Image = fetch["image"].ToString();
                    user.Coins = string.IsNullOrEmpty(fetch["coins"].ToString())
                        ? 20
                        : int.Parse(fetch["coins"].ToString());
                }
            }

            catch (PostgresException)
            {
                return null;
            }

            return user;
        }

        public bool AddUser(User user)
        {
            try
            {
                using var query =
                    new NpgsqlCommand(
                        "insert into \"user\"(username, password, name, token) values(@username, @password, @name, @token)",
                        _connection);

                query.Parameters.AddWithValue("username", user.Username);
                query.Parameters.AddWithValue("password", user.Password);
                query.Parameters.AddWithValue("name", user.Name);
                query.Parameters.AddWithValue("token", user.Token);
                return query.ExecuteNonQuery() > 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }
        public bool UpdateUser(User user)
        {
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            connection.Open();
            using var query =
                new NpgsqlCommand(
                    "update \"user\" set name = @name, bio = @bio, image = @image, coins = @coins where username = @username",
                    connection);
            query.Parameters.AddWithValue("username", user.Username);
            query.Parameters.AddWithValue("name", user.Name);
            query.Parameters.AddWithValue("bio", user.Bio);
            query.Parameters.AddWithValue("image", user.Image);
            query.Parameters.AddWithValue("coins", user.Coins);
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
        
        public bool UpdateUserForTransaction(User user, NpgsqlConnection conn, NpgsqlTransaction trans)
        {
            using var query =
                new NpgsqlCommand(
                    "update \"user\" set name = @name, bio = @bio, image = @image, coins = @coins where username = @username",
                    conn, trans);
            query.Parameters.AddWithValue("username", user.Username);
            query.Parameters.AddWithValue("name", user.Name);
            query.Parameters.AddWithValue("bio", user.Bio);
            query.Parameters.AddWithValue("image", user.Image);
            query.Parameters.AddWithValue("coins", user.Coins);
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

        /**
         * delete user from database if required
         */
        public bool DeleteUser(string uuid)
        {
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query = new NpgsqlCommand("delete from \"user\" where uuid::text = @uuid", connection);
            query.Parameters.AddWithValue("uuid", uuid);
            connection.Open();
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