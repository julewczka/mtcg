using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using mtcg.classes.entities;
using mtcg.classes.game.types.monster;
using mtcg.classes.game.types.spell;
using mtcg.repositories;
using mtcg.types;

namespace mtcg.classes.game.model
{
    public class Battle
    {
        private const int Rounds = 100;
        private User User1 { get; set; }
        private User User2 { get; set; }
        private Deck Deck1 { get; set; }
        private Deck Deck2 { get; set; }

        //TODO: Error-Handling if deck does not exist
        public Battle(User player1, User player2)
        {
            ChooseStarter(player1, player2);
            Console.WriteLine($"Starter: {player1.Username}");
            Console.WriteLine($"Second: {player2.Username}");

            var oldDeck1 = DeckRepository.GetDeckByUserUuid(User1.Id);
            oldDeck1.Cards = DeckRepository.GetCardsFromDeck(oldDeck1.Uuid);
            ;
            Deck1 = InitializeDeckInTypes(oldDeck1.Cards);
            Console.WriteLine($"Starter Deck: {ShowDeckInJson(Deck1.Cards)}");

            var oldDeck2 = DeckRepository.GetDeckByUserUuid(User2.Id);
            oldDeck2.Cards = DeckRepository.GetCardsFromDeck(oldDeck2.Uuid);
            Deck2 = InitializeDeckInTypes(oldDeck2.Cards);
            Console.WriteLine($"Seconds Deck: {ShowDeckInJson(Deck2.Cards)}");

            Console.WriteLine("StartBattle:");
            StartBattle(Deck1, Deck2);
        }

        public void StartBattle(Deck deck1, Deck deck2)
        {
            for (var i = 0; i < Rounds; i++)
            {
                Console.WriteLine();
                var randomCard1 = deck1.Cards[(new Random()).Next(0, deck1.Cards.Count)];
                var randomCard2 = deck2.Cards[(new Random()).Next(0, deck2.Cards.Count)];
                Console.WriteLine($"Round {i}");
                var round = CalcBattle(randomCard1, randomCard2);
                round.RoundCount = i;
                Console.WriteLine($"winner is {round.Winner} with {round.WinningCard.Name}, {round.WinningCard.Damage}");
                Console.WriteLine($"looser is {round.Looser} with {round.LoosingCard.Name}, {round.LoosingCard.Damage}");
                Console.WriteLine();
            }
        }

        private Round CalcBattle(Card card1, Card card2)
        {
            Console.WriteLine($"{card1.Name}, {card1.Damage} vs {card2.Name}, {card2.Damage}");
            var round = SpecialConditions(card1,card2);
            if (round.Winner != null) return round;
            
            if (card1.CardType == "monster" && card2.CardType == "monster") return BattleOnlyDamage(card1, card2);

            if (card1.CardType == "spell" || card2.CardType == "spell") return BattleWithSpells(card1, card2);
            
            return round;
        }

        private Round BattleOnlyDamage(Card card1, Card card2)
        {
            var round = new Round();
            //card1 is attacker
            if (Math.Abs(card1.Damage - card2.Damage) < 0.1)
            {
                round.Winner = User2.Username;
                round.Looser = User1.Username;
                round.WinningCard = card2;
                round.LoosingCard = card1;
            }

            if (card1.Damage > card2.Damage)
            {
                round.Winner = User1.Username;
                round.Looser = User2.Username;
                round.WinningCard = card1;
                round.LoosingCard = card2;
            }
            else
            {
                round.Winner = User2.Username;
                round.Looser = User1.Username;
                round.WinningCard = card2;
                round.LoosingCard = card1;
            }

            return round;
        }

        private Round SpecialConditions(Card card1, Card card2)
        {
            //card1 is attacker

            var round = new Round();
            //Goblin vs Dragon
            if (card1.GetType() == typeof(Goblin) && card2.GetType() == typeof(Dragon))
            {
                round.Winner = User2.Username;
                round.Looser = User1.Username;
                round.WinningCard = card2;
                round.LoosingCard = card1;
            }

            //Ork vs Wizard
            if (card1.GetType() == typeof(Ork) && card2.GetType() == typeof(Wizard))
            {
                round.Winner = User2.Username;
                round.Looser = User1.Username;
                round.WinningCard = card2;
                round.LoosingCard = card1;
            }

            //Dragon vs FireElves
            if (card1.GetType() == typeof(Dragon) &&
                (card2.GetType() == typeof(Elve) && card1.ElementType == ElementType.Fire))
            {
                round.Winner = User2.Username;
                round.Looser = User1.Username;
                round.WinningCard = card2;
                round.LoosingCard = card1;
            }

            //Knight vs WaterSpells
            if (card1.GetType() == typeof(Knight) && card2.GetType() == typeof(WaterSpell))
            {
                round.Winner = User2.Username;
                round.Looser = User1.Username;
                round.WinningCard = card2;
                round.LoosingCard = card1;
            }
            if (card1.GetType() == typeof(WaterSpell) && card2.GetType() == typeof(Knight))
            {
                round.Winner = User1.Username;
                round.Looser = User2.Username;
                round.WinningCard = card1;
                round.LoosingCard = card2;
            }

            //Kraken vs Spells
            if (card1.GetType() == typeof(Kraken) && card2 is SpellCard)
            {
                round.Winner = User1.Username;
                round.Looser = User2.Username;
                round.WinningCard = card1;
                round.LoosingCard = card2;
            }
            if (card1.GetType() == typeof(SpellCard) && card2.GetType() == typeof(Kraken))
            {
                round.Winner = User2.Username;
                round.Looser = User1.Username;
                round.WinningCard = card2;
                round.LoosingCard = card1;
            }

            return round;
        }

        private Round BattleWithSpells(Card card1, Card card2)
        {
            var c1 = new Card()
            {
                Uuid = card1.Uuid,
                Name = card1.Name,
                CardType = card1.CardType,
                ElementType = card1.ElementType,
                Damage = card1.Damage
            };
            var c2 = new Card()
            {
                Uuid = card2.Uuid,
                Name = card2.Name,
                CardType = card2.CardType,
                ElementType = card2.ElementType,
                Damage = card2.Damage
            };
            var round = new Round();
            if (card1.ElementType == card2.ElementType) return BattleOnlyDamage(c1, c2);

            switch (card1.ElementType)
            {
                //Water vs Fire
                case ElementType.Water when card2.ElementType == ElementType.Fire:
                    Console.WriteLine("effective!");
                    c1.Damage *= 2;
                    return BattleOnlyDamage(c1, c2);
                //Water vs Normal
                case ElementType.Water when card2.ElementType == ElementType.Normal:
                    Console.WriteLine("not effective!");
                    c1.Damage /= 2; 
                    return BattleOnlyDamage(c1, c2);
                //Fire vs Normal
                case ElementType.Fire when card2.ElementType == ElementType.Normal:
                    Console.WriteLine("effective!");
                    c1.Damage *= 2;
                    return BattleOnlyDamage(c1, c2);
                //Fire vs Water
                case ElementType.Fire when card2.ElementType == ElementType.Water:
                    Console.WriteLine("not effective!");
                    c1.Damage /= 2;
                    return BattleOnlyDamage(c1, c2);
                //Normal vs Water
                case ElementType.Normal when card2.ElementType == ElementType.Water:
                    Console.WriteLine("effective!");
                    c1.Damage *= 2;
                    return BattleOnlyDamage(c1, c2);
                //Normal vs Fire
                case ElementType.Normal when card2.ElementType == ElementType.Fire:
                    Console.WriteLine("not effective!");
                    c1.Damage /= 2;
                    return BattleOnlyDamage(c1, c2);
            }

            return round;
        }

        private Deck InitializeDeckInTypes(List<Card> oldDeckCards)
        {
            var newCards = new List<Card>();
            var newDeck = new Deck();
            oldDeckCards.ForEach(card =>
            {
                switch (card.CardType.ToLower())
                {
                    case "goblin":
                        var goblin = new Goblin(card.Uuid, card.Name, card.Damage, "monster", card.ElementType);
                        newCards.Add(goblin);
                        break;
                    case "dragon":
                        var dragon = new Dragon(card.Uuid, card.Name, card.Damage, "monster", card.ElementType);
                        newCards.Add(dragon);
                        break;
                    case "wizard":
                        var wizard = new Wizard(card.Uuid, card.Name, card.Damage, "monster", card.ElementType);
                        newCards.Add(wizard);
                        break;
                    case "ork":
                        var ork = new Ork(card.Uuid, card.Name, card.Damage, "monster", card.ElementType);
                        newCards.Add(ork);
                        break;
                    case "knight":
                        var knight = new Knight(card.Uuid, card.Name, card.Damage, "monster", card.ElementType);
                        newCards.Add(knight);
                        break;
                    case "kraken":
                        var kraken = new Kraken(card.Uuid, card.Name, card.Damage, "monster", card.ElementType);
                        newCards.Add(kraken);
                        break;
                    case "elve":
                        var elve = new Kraken(card.Uuid, card.Name, card.Damage, "monster", card.ElementType);
                        newCards.Add(elve);
                        break;
                    case "spell":
                        SpellCard spell = null;
                        switch (card.ElementType)
                        {
                            case ElementType.Normal:
                                spell = new NormalSpell(card.Uuid, card.Name, card.Damage, "spell", card.ElementType);
                                break;
                            case ElementType.Fire:
                                spell = new FireSpell(card.Uuid, card.Name, card.Damage, "spell", card.ElementType);
                                break;
                            case ElementType.Water:
                                spell = new WaterSpell(card.Uuid, card.Name, card.Damage, "spell", card.ElementType);
                                break;
                        }

                        newCards.Add(spell);
                        break;
                }
            });
            newDeck.Cards = newCards;
            return newDeck;
        }

        private void ChooseStarter(User player1, User player2)
        {
            var players = new List<User> {player1, player2};
            User1 = players[(new Random()).Next(0, players.Count)];
            User2 = players.Find(player => player.Id != User1.Id);
        }

        private string ShowDeckInJson(List<Card> deckCards)
        {
            var jsonString = new StringBuilder();
            deckCards.ForEach(card =>
                jsonString.Append(
                    $"CardType: {card.GetType()} {JsonSerializer.Serialize(card)}, {Environment.NewLine}"));
            return jsonString.ToString();
        }
    }
}