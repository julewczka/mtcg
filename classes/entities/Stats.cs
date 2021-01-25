namespace mtcg.classes.entities
{
    public class Stats
    {
        public string StatsUuid { get; set; }
        public string UserUuid { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Elo { get; set; }
    }
}