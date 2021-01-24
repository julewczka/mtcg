using mtcg.classes.entities;

namespace mtcg.types
{
    public struct Round
    {
        public int RoundCount { get; set; }
        public string Winner { get; set; }
        public string Looser { get; set; }
        public Card WinningCard { get; set; }
        public Card LoosingCard { get; set; }

    }
}