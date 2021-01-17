namespace mtcg
{
    public class WaterSpell : SpellCard
    {
        public WaterSpell(double damage, string cardType, ElementType elementType)
            : base(damage, cardType, elementType, ElementType.Normal)
        {
        }
    }
}