namespace mtcg
{
    public class MonsterCard : Card
    {
        protected ElementType Weakness { get; private set; }

        public MonsterCard(double damage, string cardType, ElementType elementType, ElementType weakness = ElementType.None) 
            : base(damage, cardType, elementType)
        {
            Weakness = weakness;
        }
    }
}