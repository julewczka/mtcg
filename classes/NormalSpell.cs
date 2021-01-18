namespace mtcg
{
    public class NormalSpell : SpellCard
    {
        public NormalSpell(string uuid, string name, double damage, string cardType, ElementType elementType)
            : base( uuid,name,damage, cardType, elementType)
        {
        }
    }
}