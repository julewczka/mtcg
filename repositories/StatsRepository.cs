using System;
using System.Collections.Generic;
using mtcg.classes.entities;
using mtcg.types;
using Npgsql;

namespace mtcg.repositories
{
    public class StatsRepository
    {
        public List<Stats> GetAllStats()
        {
            var score = new List<Stats>();
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query =
                new NpgsqlCommand(
                    "select row_number() over (order by elo desc, wins desc) as rank, stats_uuid, user_uuid, wins, losses, elo from stats",
                    connection);
            connection.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    var currentStats = new Stats()
                    {
                        StatsUuid = fetch["stats_uuid"].ToString(),
                        UserUuid = fetch["user_uuid"].ToString(),
                        Wins = int.Parse(fetch["wins"].ToString()),
                        Losses = int.Parse(fetch["losses"].ToString()),
                        Elo = int.Parse(fetch["elo"].ToString())
                    };
                    score.Add(currentStats);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }

            return score;
        }

        public Stats GetByUserUuid(string userUuid)
        {
            var stats = new Stats();
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query =
                new NpgsqlCommand("select * from stats where user_uuid::text = @user_uuid", connection);
            query.Parameters.AddWithValue("user_uuid", userUuid);
            connection.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    stats.StatsUuid = fetch["stats_uuid"].ToString();
                    stats.UserUuid = fetch["user_uuid"].ToString();
                    stats.Wins = int.Parse(fetch["wins"].ToString());
                    stats.Losses = int.Parse(fetch["losses"].ToString());
                    stats.Elo = int.Parse(fetch["elo"].ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }

            return stats;
        }

        public Stats GetByStatsUuid(string statsUuid)
        {
            var stats = new Stats();
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query =
                new NpgsqlCommand("select * from stats where stats_uuid::text = @stats_uuid", connection);
            query.Parameters.AddWithValue("stats_uuid", statsUuid);
            connection.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    stats.StatsUuid = fetch["stats_uuid"].ToString();
                    stats.UserUuid = fetch["user_uuid"].ToString();
                    stats.Wins = int.Parse(fetch["wins"].ToString());
                    stats.Losses = int.Parse(fetch["losses"].ToString());
                    stats.Elo = int.Parse(fetch["elo"].ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }

            return stats;
        }

        public string AddStats(Stats stats)
        {
            var statsUuid = "";
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query =
                new NpgsqlCommand(
                    "insert into stats(user_uuid, wins, losses, elo) values (@user_uuid, @wins, @losses, @elo) returning stats_uuid",
                    connection);
            query.Parameters.AddWithValue("user_uuid", Guid.Parse(stats.UserUuid));
            query.Parameters.AddWithValue("wins", stats.Wins);
            query.Parameters.AddWithValue("losses", stats.Losses);
            query.Parameters.AddWithValue("elo", stats.Elo);
            connection.Open();
            try
            {
                var fetch = query.ExecuteReader();
                while (fetch.Read())
                {
                    statsUuid = fetch["stats_uuid"].ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }

            return statsUuid;
        }

        public bool UpdateStats(Stats stats)
        {
            using var connection = new NpgsqlConnection(ConnectionString.Credentials);
            using var query =
                new NpgsqlCommand(
                    "update stats set wins = @wins, losses = @losses, elo = @elo where user_uuid = @user_uuid",
                    connection);
            query.Parameters.AddWithValue("user_uuid", Guid.Parse(stats.UserUuid));
            query.Parameters.AddWithValue("wins", stats.Wins);
            query.Parameters.AddWithValue("losses", stats.Losses);
            query.Parameters.AddWithValue("elo", stats.Elo);
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