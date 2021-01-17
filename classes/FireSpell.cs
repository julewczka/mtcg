namespace mtcg
{
    public class FireSpell : SpellCard
    {
        public FireSpell(double damage, string cardType, ElementType elementType)
            : base(damage, cardType, elementType, ElementType.Water)
        {
        }
    }
}