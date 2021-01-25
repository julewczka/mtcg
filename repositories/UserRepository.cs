using System;
using System.Collections.Generic;
using mtcg.classes.entities;
using mtcg.types;
using Npgsql;

namespace mtcg.repositories
{
    public static class UserRepository
    {
        /**
         * Get all users
         * returns a list of users
         */
        public static IEnumerable<User> SelectAll()
        {
            var retrievedUsers = new List<User>();

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString.Credentials);
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
                        Coins = double.Parse(fetch["coins"].ToString())
                    };
                    retrievedUsers.Add(currentUser);
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
        public static User SelectUserByUsername(string username)
        {
            var user = new User();
            try
            {
                using var connection = new NpgsqlConnection(ConnectionString.Credentials);
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
                        : double.Parse(fetch["coins"].ToString());
                }
            }

            catch (PostgresException)
            {
                return null;
            }

            return user;
        }
        
        public static User SelectUserByUuid(string uuid)
        {
            var user = new User();
            
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.Credentials))
                {
                    using var query = new NpgsqlCommand("select * from \"user\" where uuid::text = @uuid", connection);
                    query.Parameters.AddWithValue("uuid", uuid);
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

        public static User SelectUserByToken(string token)
        {
            var user = new User();
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.Credentials))
                {
                    using var query = new NpgsqlCommand("select * from \"user\" where token = @token", connection);
                    query.Parameters.AddWithValue("token", token);
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
        /**
         * insert a user into database table "user"
         */
        public static bool InsertUser(User user)
        {
            var success = false;
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.Credentials))
                {
                    var insertCount = 0;
                    using var query =
                        new NpgsqlCommand(
                            "insert into \"user\"(username, password, name, token) values(@username, @password, @name, @token)",
                            connection);

                    query.Parameters.AddWithValue("username", user.Username);
                    query.Parameters.AddWithValue("password", user.Password);
                    query.Parameters.AddWithValue("name", user.Name);
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
        
        /**
         * edit user properties
         */
        public static bool UpdateUser(User user)
        {
            var success = false;
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            try
            {
                var updateCount = 0;
                using var query =
                    new NpgsqlCommand(
                        "update \"user\" set name = @name, bio = @bio, image = @image, coins = @coins where username = @username",
                        connection);
                query.Parameters.AddWithValue("username", user.Username);
                query.Parameters.AddWithValue("name", user.Name);
                query.Parameters.AddWithValue("bio", user.Bio);
                query.Parameters.AddWithValue("image", user.Image);
                query.Parameters.AddWithValue("coins", user.Coins);
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
        
        /**
         * delete user from database if required
         */
        public static bool DeleteUser(string uuid)
        {
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            try
            {
                using var query = new NpgsqlCommand("delete from \"user\" where uuid::text = @uuid", connection);
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