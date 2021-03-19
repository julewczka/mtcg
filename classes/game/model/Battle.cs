using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using mtcg.classes.entities;
using mtcg.classes.game.types.monster;
using mtcg.classes.game.types.spell;
using mtcg.controller;
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
        private Stats Stats1 { get; set; }
        private Stats Stats2 { get; set; }
        private readonly StringBuilder _battleLog = new();
        private readonly PackageRepository _packRepo;
        private readonly DeckRepository _deckRepo;
        private readonly UserRepository _userRepo;
        private readonly StatsRepository _statsRepo;
        private readonly StatsController _statsCtrl;
        
        
        public Battle(User player1, User player2)
        {
            _packRepo = new PackageRepository();
            _deckRepo = new DeckRepository();
            _userRepo = new UserRepository();
            _statsRepo = new StatsRepository();

            _statsCtrl = new StatsController();
            
            if (player1?.Id == null || player2?.Id == null) return;
            
            var nl = Environment.NewLine;
            ChooseStarter(player1, player2);
            _battleLog.Append($"Starter: {User1.Username}{nl}");
            _battleLog.Append($"Second: {User2.Username}{nl}");

            var oldDeck1 = _deckRepo.GetDeckByUserUuid(User1.Id);
            oldDeck1.Cards = _deckRepo.GetCardsFromDeck(oldDeck1.Uuid);

            Deck1 = InitializeDeckInTypes(oldDeck1.Cards);
            _battleLog.Append($"Starter Deck: {Deck1.Uuid} {nl} {ShowDeckInJson(Deck1.Cards)} {nl}");

            var oldDeck2 = _deckRepo.GetDeckByUserUuid(User2.Id);
            oldDeck2.Cards = _deckRepo.GetCardsFromDeck(oldDeck2.Uuid);
            Deck2 = InitializeDeckInTypes(oldDeck2.Cards);
            _battleLog.Append($"Seconds Deck: {Deck2.Uuid} {nl} {ShowDeckInJson(Deck2.Cards)} {nl}");

            Stats1 = SetupStats(User1.Id);
            Stats2 = SetupStats(User2.Id);
        }

        public Response StartBattle()
        {
            var nl = Environment.NewLine;
            var content = new StringBuilder(_battleLog + nl);

            if (Deck1?.Cards == null) return RTypes.Forbidden;
            if (Deck2?.Cards == null) return RTypes.Forbidden;

            content.Append("Start battle:" + nl);

            for (var i = 1; i <= Rounds; i++)
            {
                content.Append($"Round {i}" + nl);

                var randomCard1 = Deck1.Cards[(new Random()).Next(0, Deck1.Cards.Count)];
                var randomCard2 = Deck2.Cards[(new Random()).Next(0, Deck2.Cards.Count)];

                content.Append($"{User1.Username}'s deck size: {Deck1.Cards.Count}" + nl);
                content.Append($"{User2.Username}'s deck size: {Deck2.Cards.Count}" + nl);

                var round = CalcBattle(randomCard1, randomCard2);
                content.Append(
                    $"{randomCard1.Name}({randomCard1.Damage}) attacks {randomCard2.Name}({randomCard2.Damage})" + nl);

                round.RoundCount = i;

                SwitchCardFromDeck(round.LoosingCard, round.WinningDeck, round.LoosingDeck);
                content.Append(
                    $"winner is {round.Winner} with {round.WinningCard.Name}, {round.WinningCard.Damage} and {round.LoosingCard.Name} is added to the deck {round.WinningDeck.Uuid}" +
                    $"{nl}{User1.Username}'s deck size after round {i}: {Deck1.Cards.Count}{nl}"
                );
                content.Append(
                    $"looser is {round.Looser} with {round.LoosingCard.Name}, {round.LoosingCard.Damage} and {round.LoosingCard.Name} is removed from the deck {round.LoosingDeck.Uuid}" +
                    $"{nl}{User2.Username}'s deck size after round {i}: {Deck2.Cards.Count}{nl}{nl}"
                );

                if (round.LoosingDeck.Cards.Count == 0)
                {
                    content.Append($"{round.Winner} won the game!{nl}" +
                                   $"{round.Winner} gained 5 coins");
                    UpdateStats(round.Winner);
                    return AddCoins(round.Winner, content.ToString());
                }
            }

            return RTypes.CResponse(content.ToString(), 200, "text/plain");
        }

        private void UpdateStats(string winnerUuid)
        {
            if (Stats1.UserUuid == winnerUuid)
            {
                Stats1.Wins += 1;
                Stats1.Elo += 3;
                Stats2.Losses += 1;
                Stats2.Elo -= 5;
            }
            else
            {
                Stats2.Wins += 1;
                Stats2.Elo += 3;
                Stats1.Losses += 1;
                Stats1.Elo -= 5;
            }

            _statsRepo.UpdateStats(Stats1);
            _statsRepo.UpdateStats(Stats2);
        }

        private Stats SetupStats(string userUuid)
        {
            var stats = _statsRepo.GetByUserUuid(userUuid);
            if (stats?.UserUuid != null) return stats;

            var statsUuid = _statsCtrl.CreateStatsIfNotExist(User1.Id);
            stats = _statsRepo.GetByStatsUuid(statsUuid);

            return stats;
        }

        private Response AddCoins(string winnerUuid, string content)
        {
            var updateUser = _userRepo.GetByUsername(winnerUuid);
            updateUser.Coins += 5;
            return !_userRepo.Update(updateUser)
                ? RTypes.CError("Something went wrong", 403)
                : RTypes.CResponse(content, 200, "text/plain");
        }

        private void SwitchCardFromDeck(Card card, Deck winDeck, Deck looseDeck)
        {
            looseDeck.DeleteCardFromDeck(card);
            winDeck.AddCardToDeck(card);
            if (winDeck.Uuid == Deck1.Uuid)
            {
                Deck1 = winDeck;
                Deck2 = looseDeck;
            }
            else
            {
                Deck1 = looseDeck;
                Deck2 = winDeck;
            }
        }

        public Round CalcBattle(Card card1, Card card2)
        {
            var round = SpecialConditions(card1, card2);
            if (round.Winner != null) return round;

            if (card1.CardType == "monster" && card2.CardType == "monster") return BattleOnlyDamage(card1, card2);

            if (card1.CardType == "spell" || card2.CardType == "spell") return BattleWithSpells(card1, card2);

            return round;
        }

        private Round BattleOnlyDamage(Card card1, Card card2, Card originalCard1 = null, Card originalCard2 = null)
        {
            originalCard1 ??= card1;
            originalCard2 ??= card2;
            var round = new Round();
            //card1 is attacker
            if (Math.Abs(card1.Damage - card2.Damage) < 0.1)
            {
                round.Winner = User2.Username;
                round.Looser = User1.Username;
                round.WinningCard = originalCard2;
                round.LoosingCard = originalCard1;
                round.WinningDeck = Deck2;
                round.LoosingDeck = Deck1;
            }

            if (card1.Damage > card2.Damage)
            {
                round.Winner = User1.Username;
                round.Looser = User2.Username;
                round.WinningCard = originalCard1;
                round.LoosingCard = originalCard2;
                round.WinningDeck = Deck1;
                round.LoosingDeck = Deck2;
            }
            else
            {
                round.Winner = User2.Username;
                round.Looser = User1.Username;
                round.WinningCard = originalCard2;
                round.LoosingCard = originalCard1;
                round.WinningDeck = Deck2;
                round.LoosingDeck = Deck1;
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
                round.WinningDeck = Deck2;
                round.LoosingDeck = Deck1;
            }

            //Dragon vs FireElves
            if (card1.GetType() == typeof(Dragon) &&
                (card2.GetType() == typeof(Elve) && card2.ElementType == ElementType.Fire))
            {
                round.Winner = User2.Username;
                round.Looser = User1.Username;
                round.WinningCard = card2;
                round.LoosingCard = card1;
                round.WinningDeck = Deck2;
                round.LoosingDeck = Deck1;
            }

            //Knight vs WaterSpells
            if (card1.GetType() == typeof(Knight) && card2.GetType() == typeof(WaterSpell))
            {
                round.Winner = User2.Username;
                round.Looser = User1.Username;
                round.WinningCard = card2;
                round.LoosingCard = card1;
                round.WinningDeck = Deck2;
                round.LoosingDeck = Deck1;
            }

            if (card1.GetType() == typeof(WaterSpell) && card2.GetType() == typeof(Knight))
            {
                round.Winner = User1.Username;
                round.Looser = User2.Username;
                round.WinningCard = card1;
                round.LoosingCard = card2;
                round.WinningDeck = Deck1;
                round.LoosingDeck = Deck2;
            }

            //Kraken vs Spells
            if (card1.GetType() == typeof(Kraken) && card2 is SpellCard)
            {
                round.Winner = User1.Username;
                round.Looser = User2.Username;
                round.WinningCard = card1;
                round.LoosingCard = card2;
                round.WinningDeck = Deck1;
                round.LoosingDeck = Deck2;
            }

            if (card1 is SpellCard && card2.GetType() == typeof(Kraken))
            {
                round.Winner = User2.Username;
                round.Looser = User1.Username;
                round.WinningCard = card2;
                round.LoosingCard = card1;
                round.WinningDeck = Deck2;
                round.LoosingDeck = Deck1;
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
            if (card1.ElementType == card2.ElementType) return BattleOnlyDamage(card1, card2);

            switch (card1.ElementType)
            {
                //Water vs Fire
                case ElementType.Water when card2.ElementType == ElementType.Fire:
                    c1.Damage *= 2;
                    return BattleOnlyDamage(c1, c2, card1, card2);
                //Water vs Normal
                case ElementType.Water when card2.ElementType == ElementType.Normal:
                    c1.Damage /= 2;
                    return BattleOnlyDamage(c1, c2, card1, card2);
                //Fire vs Normal
                case ElementType.Fire when card2.ElementType == ElementType.Normal:
                    c1.Damage *= 2;
                    return BattleOnlyDamage(c1, c2, card1, card2);
                //Fire vs Water
                case ElementType.Fire when card2.ElementType == ElementType.Water:
                    c1.Damage /= 2;
                    return BattleOnlyDamage(c1, c2, card1, card2);
                //Normal vs Water
                case ElementType.Normal when card2.ElementType == ElementType.Water:
                    c1.Damage *= 2;
                    return BattleOnlyDamage(c1, c2, card1, card2);
                //Normal vs Fire
                case ElementType.Normal when card2.ElementType == ElementType.Fire:
                    c1.Damage /= 2;
                    return BattleOnlyDamage(c1, c2, card1, card2);
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