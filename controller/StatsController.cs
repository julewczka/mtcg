using System.Text;
using System.Text.Json;
using mtcg.classes.entities;
using mtcg.repositories;

namespace mtcg.controller
{
    public class StatsController
    {
        private readonly StatsRepository _statsRepo;
        public StatsController()
        {
            _statsRepo = new StatsRepository();
        }
        public Response Get(User user)
        {
            var stats = _statsRepo.GetByUserUuid(user.Id);
            if (stats?.StatsUuid == null) return RTypes.Forbidden;

            var content = new StringBuilder();
            content.Append(JsonSerializer.Serialize(stats));
            return RTypes.CResponse(content.ToString(), 200, "application/json");
        }
        
        public string CreateStatsIfNotExist(string userUuid)
        {
            var createStats = new Stats()
            {
                UserUuid = userUuid,
                Wins = 0,
                Losses = 0,
                Elo = 100,
            };
            return _statsRepo.AddStats(createStats);
        }
    }
}