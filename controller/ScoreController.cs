using System;
using System.Text;
using System.Text.Json;
using mtcg.repositories;

namespace mtcg.controller
{
    public class ScoreController
    {
        private readonly StatsRepository _statsRepo;
        public ScoreController()
        {
            _statsRepo = new StatsRepository();
        }
        public Response GetScore()
        {
            var scores = _statsRepo.GetAllStats();
            var content = new StringBuilder();
            if (scores == null || scores.Count == 0) return RTypes.CError("scoreboard not found", 404);
            scores.ForEach(score => content.Append(JsonSerializer.Serialize(score) + "," + Environment.NewLine));
            return RTypes.CResponse(content.ToString(), 200, "application/json");
        }
    }
}