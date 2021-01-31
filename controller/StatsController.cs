using System.Text;
using System.Text.Json;
using mtcg.classes.entities;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class StatsController
    {
        public static Response Get(User user)
        {
            var stats = StatsRepository.GetByUserUuid(user.Id);
            if (stats?.StatsUuid == null) return RTypes.Forbidden;

            var content = new StringBuilder();
            content.Append(JsonSerializer.Serialize(stats));
            return RTypes.CResponse(content.ToString(), 200, "application/json");
        }
        
        public static string CreateStatsIfNotExist(string userUuid)
        {
            var createStats = new Stats()
            {
                UserUuid = userUuid,
                Wins = 0,
                Losses = 0,
                Elo = 100,
            };
            return StatsRepository.AddStats(createStats);
        }
    }
}