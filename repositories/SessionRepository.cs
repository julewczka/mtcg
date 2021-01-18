using System;
using Npgsql;

namespace mtcg.repositories
{
    public static class SessionRepository
    {
        private const string Credentials =
            "Server=127.0.0.1;Port=5432;Database=mtcg-db;User Id=mtcg-user;Password=mtcg-pw";

        public static void LogLogin(string username, DateTime timestamp)
        {
            try
            {
                using (var connection = new NpgsqlConnection(Credentials))
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

        public static User GetUserByName(string username)
        {
            var user = new User();
            try
            {
                using (var connection = new NpgsqlConnection(Credentials))
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