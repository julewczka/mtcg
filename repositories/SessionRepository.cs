using System;
using System.Collections.Generic;
using mtcg.classes.entities;
using mtcg.types;
using Npgsql;

namespace mtcg.repositories
{
    public static class SessionRepository
    {

        /**
         * Get all records of the db-table "logins"
         * returns a list of Sessions (username + timestamp)
         */
        public static List<Session> GetLogs()
        {
            var retrievedLogs = new List<Session>();
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.Credentials))
                {
                    connection.Open();
                    using var query = new NpgsqlCommand("select * from logins", connection);
                    var fetch = query.ExecuteReader();
                    while (fetch.Read())
                    {
                        var session = new Session()
                        {
                            Uuid = fetch["uuid"].ToString(),
                            Username = fetch["username"].ToString(),
                            Timestamp = DateTime.Parse(fetch["timestamp"].ToString())
                        };
                        retrievedLogs.Add(session);
                    }
                }
            }
            catch (PostgresException)
            {
                return null;
            }

            return retrievedLogs;
        }
        
        /**
         * Insert a session (username + timestamp) after each login
         */
        public static void LogLogin(string username, DateTime timestamp)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.Credentials))
                {
                    using var query =
                        new NpgsqlCommand("insert into logins(username, timestamp) values (@username, @timestamp)",
                            connection);
                    query.Parameters.AddWithValue("username", username);
                    query.Parameters.AddWithValue("timestamp", timestamp);
                    connection.Open();
                    if (query.ExecuteNonQuery() > 0) Console.WriteLine("Log to Session - Nothing has changed!");
                }
            }
            catch (PostgresException)
            {
                Console.WriteLine("Log to Session - Error!");
            }
        }

        /**
         * Get user by username
         */
        public static User GetUserByName(string username)
        {
            var user = new User();
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.Credentials))
                {
                    using var query = new NpgsqlCommand("select username, password from \"user\" where username = @p",
                        connection);
                    query.Parameters.AddWithValue("p", username);
                    connection.Open();
                    var fetch = query.ExecuteReader();
                    while (fetch.Read())
                    {
                        user.Username = fetch["username"].ToString();
                        user.Password = fetch["password"].ToString();
                    }
                }
            }
            catch (PostgresException)
            {
                return null;
            }

            return user;
        }
    }
}