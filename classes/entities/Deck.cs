using System;
using System.Collections.Generic;

namespace mtcg.classes.entities
{
    public class Deck
    {
        public string Uuid { get; set; }
        public List<Card> Cards { get; set; }

        public void DeleteCardFromDeck(Card card)
        {
            var newList = new List<Card>(Cards);
            newList.ForEach(c =>
            {
                if (c.Uuid == card.Uuid)
                {
                    Cards.Remove(c);
                }
            });

        }

        public void AddCardToDeck(Card card)
        {
            Cards.Add(card);
        }
        
    }
}