using System;
using System.Text;
using System.Text.Json;
using mtcg.repositories;

namespace mtcg.controller
{
    public class ScoreController
    {
        public static Response GetScore()
        {
            var scores = StatsRepository.SelectAllStats();
            var content = new StringBuilder();
            if (scores == null || scores.Count == 0) return ResponseTypes.CustomError("scoreboard not found", 404);
            scores.ForEach(score => content.Append(JsonSerializer.Serialize(score) + "," + Environment.NewLine));
            return ResponseTypes.CustomResponse(content.ToString(), 200, "application/json");
        }
    }
}