﻿namespace mtcg.classes.game.types.spell
{
    public class FireSpell : SpellCard
    {
        public FireSpell(string uuid, string name, double damage, string cardType, ElementType elementType)
            : base( uuid,name,damage, cardType, elementType)
        {
        }
    }
}