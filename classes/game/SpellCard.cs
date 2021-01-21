namespace mtcg
{
    public class SpellCard : Card
    {
        public SpellCard(string uuid, string name, double damage, string cardType, ElementType elementType) 
            : base(uuid, name, damage, cardType, elementType)
        {
        }

    }
}    