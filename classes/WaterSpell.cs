namespace mtcg
{
    public class WaterSpell : SpellCard
    {
        public WaterSpell(string uuid, string name, double damage, string cardType, ElementType elementType)
            : base( uuid,name,damage, cardType, elementType)
        {
        }
    }
}