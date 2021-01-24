namespace mtcg.classes.entities
{
    public class Card
    {
        public string Uuid { get; set; }
        public string Name { get; set; }
        public double Damage { get; set; }
        public string CardType { get; set; }
        public ElementType ElementType { get; set; }

        public Card()
        {
            
        }
        public Card(string uuid, string name, double damage, string cardType, ElementType elementType)
        {
            Uuid = uuid;
            Name = name;
            Damage = damage;
            CardType = cardType;
            ElementType = elementType;
        }
        
        public static ElementType GetElementType(string elementType)
        {
            return elementType.ToLower() switch
            {
                "normal" => ElementType.Normal,
                "water" => ElementType.Water,
                "fire" => ElementType.Fire,
                _ => ElementType.None
            };
        }
    }
}