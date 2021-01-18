using System;
using System.Collections.Generic;
using Npgsql;

namespace mtcg.repositories
{
    public static class UserRepository
    {
        private const string Credentials =
            "Server=127.0.0.1;Port=5432;Database=mtcg-db;User Id=mtcg-user;Password=mtcg-pw";

        /**
         * Get all users
         * returns a list of users
         */
        public static IEnumerable<User> SelectAll()
        {
            var retrievedUsers = new List<User>();

            try
            {
                using (var connection = new NpgsqlConnection(Credentials))
                {
                    using var query = new NpgsqlCommand("select * from \"user\"", connection);
                    connection.Open();

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
                            Coins = string.IsNullOrEmpty(fetch["coins"].ToString())
                                ? 20
                                : int.Parse(fetch["coins"].ToString())
                        };
                        retrievedUsers.Add(currentUser);
                    }
                }
            }
            catch (PostgresException)
            {
                retrievedUsers = new List<User>();
            }
            
            return retrievedUsers;
        }

        /**
         * Get a single user
         * returns an user object
         */
        public static User SelectUser(string username)
        {
            var user = new User();
            try
            {
                using (var connection = new NpgsqlConnection(Credentials))
                {
                    using var query = new NpgsqlCommand("select * from \"user\" where username = @p", connection);
                    query.Parameters.AddWithValue("p", username);
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
                            : int.Parse(fetch["coins"].ToString());
                    }
                }
            }

            catch (PostgresException)
            {
                return null;
            }

            return user;
        }
        
        public static bool InsertUser(User user)
        {
            var success = false;
            try
            {
                using (var connection = new NpgsqlConnection(Credentials))
                {
                    var insertCount = 0;
                    using var query =
                        new NpgsqlCommand(
                            "insert into \"user\"(username, password, name, token) values(@username, @password, @name, @token)",
                            connection);

                    query.Parameters.AddWithValue("username", user.Username);
                    query.Parameters.AddWithValue("password", user.Password);
                    query.Parameters.AddWithValue("name", user.Username);
                    query.Parameters.AddWithValue("token", user.Token);
                    connection.Open();
                    insertCount = query.ExecuteNonQuery();
                    if (insertCount > 0) success = true;
                }
            }
            catch (PostgresException)
            {
                success = false;
            }

            return success;
        }
        
        public static bool UpdateUser(User user)
        {
            var success = false;
            using var connection = new NpgsqlConnection(Credentials);
            try
            {
                var updateCount = 0;
                using var query =
                    new NpgsqlCommand(
                        "update \"user\" set name = @name, bio = @bio, image = @image where username = @username",
                        connection);
                query.Parameters.AddWithValue("username", user.Username);
                query.Parameters.AddWithValue("name", user.Name);
                query.Parameters.AddWithValue("bio", user.Bio);
                query.Parameters.AddWithValue("image", user.Image);
                connection.Open();
                updateCount = query.ExecuteNonQuery();
                if (updateCount > 0) success = true;
            }
            catch (PostgresException)
            {
                success = false;
            }

            return success;
        }
        
        public static bool DeleteUser(string uuid)
        {
            var success = false;
            using var connection = new NpgsqlConnection(Credentials);
            try
            {
                var deleteCount = 0;
                using var query = new NpgsqlCommand("delete from \"user\" where uuid::text = @uuid", connection);
                query.Parameters.AddWithValue("uuid", uuid);
                connection.Open();
                deleteCount = query.ExecuteNonQuery();
                if (deleteCount > 0) success = true;
            }
            catch (PostgresException)
            {
                success = false;
            }

            return success;
        }
    }
}