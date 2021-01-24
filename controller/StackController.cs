using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class StackController
    {
        private static readonly List<Card> LockList = new();

        public static Response Get(string token)
        {
            var data = new StringBuilder();

            var user = UserRepository.SelectUserByToken(token);
            if (user == null) return ResponseTypes.NotFoundRequest;
           
            var listStack = StackRepository.GetStack(user.Id);
            if (listStack == null) return ResponseTypes.NotFoundRequest;
            
            listStack.ForEach(card => { data.Append(JsonSerializer.Serialize(card)); });

            return ResponseTypes.CustomResponse(data.ToString(), 200, "application/json");
        }

        public static bool IsLocked(Card card)
        {
            return LockList.Contains(card);
        }
    }
}