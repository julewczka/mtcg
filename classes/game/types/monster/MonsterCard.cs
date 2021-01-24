
using mtcg.classes.entities;

namespace mtcg.classes.game.types.monster
{
    public class MonsterCard : Card
    {

        public MonsterCard(string uuid, string name,double damage, string cardType, ElementType elementType) 
            : base(uuid, name,damage, cardType, elementType)
        {
        }
    }
}