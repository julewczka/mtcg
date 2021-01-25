using mtcg.classes.game.model;
using mtcg.classes.game.types.monster;
using mtcg.classes.game.types.spell;
using mtcg.controller;
using mtcg.repositories;
using Xunit;

namespace mtcg
{
    public class BattleTest
    {
            [Fact]
            public void IsBattleListEmptyAfter2Joins()
            {
                var token1 = "julewczka-mtcgToken";
                var token2 = "wmattisssen9-mtcgToken";

                BattleController.Post(token1);
                BattleController.Post(token2);
                Assert.Empty(BattleController.GetBattleList());
            }
            
            [Fact]
            public void IsBattleListCountAfter3Joins()
            {
                var token1 = "julewczka-mtcgToken";
                var token2 = "wmattisssen9-mtcgToken";
                var token3 = "admin-mtcgToken";

                BattleController.Post(token1);
                BattleController.Post(token2);
                BattleController.Post(token3);
                Assert.Single(BattleController.GetBattleList());
            }

            [Fact]
            public void IsBattleSuccessful()
            {
                var token1 = "julewczka-mtcgToken";
                var token2 = "wmattisssen9-mtcgToken";
                var user1 = UserRepository.SelectUserByToken(token1);
                var user2 = UserRepository.SelectUserByToken(token2);

                var battle = new Battle(user1, user2);
                Assert.Equal(200, battle.StartBattle().StatusCode);
            }

            [Fact]
            public void SendUnregisteredToken()
            {
                var token1 = "julewczka-mtcgToken";
                var token2 = "wmattisssen9-mtcgToken";
                var user1 = UserRepository.SelectUserByToken(token1);
                var user2 = UserRepository.SelectUserByToken(token2);

                var battle = new Battle(user1, user2);
                Assert.Equal(403, battle.StartBattle().StatusCode);
            }

            /// <summary>
            /// Monster vs Monster
            /// </summary>
            [Fact]
            public void DragonVsKnight()
            {
                var dragon = new Dragon("99f8f8dc-e25e-4a95-aa2c-782823f36e2a", "NormalDragon", 50, "monster",
                    ElementType.Normal);
                var knight = new Knight("fe5a30b2-3e7a-49fc-babf-5647e1d12f39", "FireKnight", 40, "monster",
                    ElementType.Fire);
                
                var token1 = "julewczka-mtcgToken";
                var token2 = "wmattisssen9-mtcgToken";
                var user1 = UserRepository.SelectUserByToken(token1);
                var user2 = UserRepository.SelectUserByToken(token2);

                var battle = new Battle(user1, user2);
                var round = battle.CalcBattle(dragon, knight);
                Assert.Equal(round.WinningCard, dragon);
            }
            
            [Fact]
            public void GoblinVsDragon()
            {
                var dragon = new Dragon("99f8f8dc-e25e-4a95-aa2c-782823f36e2a", "NormalDragon", 50, "monster",
                    ElementType.Normal);
                var goblin = new Goblin("b0d9aac3-1227-4795-8b19-8ceab5f10ace", "FireGoblin", 60, "monster",
                    ElementType.Fire);
                
                var token1 = "julewczka-mtcgToken";
                var token2 = "wmattisssen9-mtcgToken";
                var user1 = UserRepository.SelectUserByToken(token1);
                var user2 = UserRepository.SelectUserByToken(token2);

                var battle = new Battle(user1, user2);
                var round = battle.CalcBattle(goblin, dragon);
                Assert.Equal(round.WinningCard, dragon);
            }
            
            /// <summary>
            /// Kraken should win, even when the damage of kraken is halved because of ElementType
            /// </summary>
            [Fact]
            public void KrakenVsNormalSpell()
            {
                var kraken = new Kraken("8af539f8-53ff-4c77-baf4-8ccbbfc6584b", "WaterKraken", 30, "monster",
                    ElementType.Water);
                var normalSpell = new NormalSpell("ccf4d64a-4249-4d20-895c-35325e35706f", "NormalSpell", 30, "spell",
                    ElementType.Normal);
                
                var token1 = "julewczka-mtcgToken";
                var token2 = "wmattisssen9-mtcgToken";
                var user1 = UserRepository.SelectUserByToken(token1);
                var user2 = UserRepository.SelectUserByToken(token2);

                var battle = new Battle(user1, user2);
                var round = battle.CalcBattle(kraken, normalSpell);
                Assert.Equal(round.WinningCard, kraken);
            }
            
            /// <summary>
            /// Wizard should win, even when the damage of Ork is higher
            /// </summary>
            [Fact]
            public void OrkVsWizard()
            {
                var ork = new Ork("8af539f8-53ff-4c77-baf4-8ccbbfc6584b", "FireOrk", 45, "monster",
                    ElementType.Fire);
                var wizard = new Wizard("ccf4d64a-4249-4d20-895c-35325e35706f", "NormalWizard", 30, "monster",
                    ElementType.Normal);
                
                var token1 = "julewczka-mtcgToken";
                var token2 = "wmattisssen9-mtcgToken";
                var user1 = UserRepository.SelectUserByToken(token1);
                var user2 = UserRepository.SelectUserByToken(token2);

                var battle = new Battle(user1, user2);
                var round = battle.CalcBattle(ork, wizard);
                Assert.Equal(round.WinningCard, wizard);
            }

            /// <summary>
            /// Knight will drown
            /// </summary>
            [Fact]
            public void KnightsVsWaterSpell()
            {
                var knight = new Knight("8af539f8-53ff-4c77-baf4-8ccbbfc6584b", "NormalKnight", 45, "monster",
                    ElementType.Normal);
                var waterSpell = new WaterSpell("ccf4d64a-4249-4d20-895c-35325e35706f", "WaterSpell", 30, "spell",
                    ElementType.Normal);

                var token1 = "julewczka-mtcgToken";
                var token2 = "wmattisssen9-mtcgToken";
                var user1 = UserRepository.SelectUserByToken(token1);
                var user2 = UserRepository.SelectUserByToken(token2);

                var battle = new Battle(user1, user2);
                var round = battle.CalcBattle(knight, waterSpell);
                Assert.Equal(round.WinningCard, waterSpell);
            }
            
            /// <summary>
            /// Knight will win
            /// </summary>
            [Fact]
            public void KnightsVsNormalSpell()
            {
                var knight = new Knight("8af539f8-53ff-4c77-baf4-8ccbbfc6584b", "NormalKnight", 45, "monster",
                    ElementType.Normal);
                var normalSpell = new NormalSpell("ccf4d64a-4249-4d20-895c-35325e35706f", "NormalSpell", 30, "spell",
                    ElementType.Normal);

                var token1 = "julewczka-mtcgToken";
                var token2 = "wmattisssen9-mtcgToken";
                var user1 = UserRepository.SelectUserByToken(token1);
                var user2 = UserRepository.SelectUserByToken(token2);

                var battle = new Battle(user1, user2);
                var round = battle.CalcBattle(knight, normalSpell);
                Assert.Equal(round.WinningCard, knight);
            }
            
            /// <summary>
            /// FireElve will evade Dragons attacks
            /// </summary>
            [Fact]
            public void DragonVsFireElve()
            {
                var dragon = new Dragon("8af539f8-53ff-4c77-baf4-8ccbbfc6584b", "WaterDragon", 50, "monster",
                    ElementType.Water);
                var fireElve = new Elve("ccf4d64a-4249-4d20-895c-35325e35706f", "FireElve", 30, "monster",
                    ElementType.Fire);

                var token1 = "julewczka-mtcgToken";
                var token2 = "wmattisssen9-mtcgToken";
                var user1 = UserRepository.SelectUserByToken(token1);
                var user2 = UserRepository.SelectUserByToken(token2);

                var battle = new Battle(user1, user2);
                var round = battle.CalcBattle(dragon, fireElve);
                Assert.Equal(round.WinningCard, fireElve);
            }
    }
}