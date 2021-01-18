namespace mtcg
{
    public class Card
    {
        public string Uuid { get; set; }
        
        public string Name { get; set; }
        public double Damage { get; private set; }
        public string CardType { get; private set; }
        
        public ElementType ElementType { get; set; }

        public Card(string uuid, string name, double damage, string cardType, ElementType elementType)
        {
            Uuid = uuid;
            Name = name;
            Damage = damage;
            CardType = cardType;
            ElementType = elementType;
        }
    }
}