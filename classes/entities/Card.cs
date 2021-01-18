namespace mtcg
{
    public class Card
    {
        public string Uuid { get; set; }
        public double Damage { get; private set; }
        public string CardType { get; private set; }

        public Card(string uuid, double damage, string cardType)
        {
            Uuid = uuid;
            Damage = damage;
            CardType = cardType;
        }
    }
}