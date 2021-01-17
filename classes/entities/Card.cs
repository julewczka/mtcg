namespace mtcg
{
    public class Card
    {
        private int Id { get; set; }
        protected double Damage { get; private set; }
        protected string CardType { get; private set; }
        protected ElementType ElementType { get; private set; }

        public Card(double damage, string cardType, ElementType elementType)
        {
            Damage = damage;
            CardType = cardType;
            ElementType = elementType;
        }
    }
}