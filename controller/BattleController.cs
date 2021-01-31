using System;
using System.Collections.Generic;
using mtcg.classes.entities;
using mtcg.classes.game.model;
using mtcg.classes.game.types.monster;
using mtcg.classes.game.types.spell;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class BattleController
    {
        private static readonly List<User> BattleList = new();
        private static readonly object BattleLock = new();

        public static Response Post(string token)
        {
            var user = UserRepository.SelectUserByToken(token);
            if(BattleList.Count < 2) BattleList.Add(user);

            if (BattleList.Count == 2)
            {
                var player1 = BattleList[0];
                var player2 = BattleList[1];
                BattleList.Clear();
                var battle = new Battle(player1, player2);
                lock (BattleLock)
                {
                   return battle.StartBattle();
                }
            }
            return RTypes.CResponse("waiting...", 200, "text/plain");
        }

        public static List<User> GetBattleList()
        {
            return BattleList;
        }
    }
}