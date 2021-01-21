namespace mtcg
{
    public class MonsterCard : Card
    {

        public MonsterCard(string uuid, string name,double damage, string cardType, ElementType elementType) 
            : base(uuid, name,damage, cardType, elementType)
        {
        }
    }
}