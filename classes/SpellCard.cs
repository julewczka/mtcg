namespace mtcg
{
    public class SpellCard : Card
    {
        protected ElementType Weakness { get; private set; }

        public SpellCard(double damage, string cardType, ElementType elementType, ElementType weakness) 
            : base(damage, cardType, elementType)
        {
            Weakness = weakness;
        }

    }
}    