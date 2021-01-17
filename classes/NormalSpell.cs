namespace mtcg
{
    public class NormalSpell : SpellCard
    {
        public NormalSpell(double damage, string cardType, ElementType elementType)
            : base(damage, cardType, elementType, ElementType.Fire)
        {
        }
    }
}