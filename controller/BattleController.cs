using System.Collections.Generic;
using mtcg.classes.entities;
using mtcg.classes.game.model;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class BattleController
    {
        private static readonly List<User> BattleList = new();

        public static Response Post(string token)
        {
            var user = UserRepository.SelectUserByToken(token);
            if(BattleList.Count < 2) BattleList.Add(user);
            
            if (BattleList.Count == 2)
            {
                var player1 = BattleList[0];
                var player2 = BattleList[1];
                var battle = new Battle(player1, player2);
                
                BattleList.Clear();
                
            }
            return ResponseTypes.HttpOk;
        }
    }
}