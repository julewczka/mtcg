using System.Collections.Generic;

namespace mtcg.classes.entities
{
    public class Deck
    {
        public string Uuid { get; set; }
        public List<Card> Cards { get; set; }
    }
}