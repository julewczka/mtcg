using System.Collections.Generic;

namespace mtcg
{
    public class Package
    {
        public string Uuid { get; set; }
        public List<Card> Cards { get; set; }
        public double Price { get; set; }
    }
}