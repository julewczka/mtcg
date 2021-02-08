using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using mtcg.classes.entities;
using mtcg.repositories;

namespace mtcg.controller
{
    public class StackController
    {
        private static readonly List<Card> LockList = new();
        private readonly StackRepository _stackRepo;

        public StackController()
        {
            _stackRepo = new StackRepository();
        }
        public Response Get(User user)
        {
            if (user?.Id == null) return RTypes.NotFoundRequest;
           
            var data = new StringBuilder();
            var listStack = _stackRepo.GetStack(user.Id);
            if (listStack == null) return RTypes.NotFoundRequest;
            
            listStack.ForEach(card => { data.Append(JsonSerializer.Serialize(card)); });

            return RTypes.CResponse(data.ToString(), 200, "application/json");
        }

        public bool IsLocked(Card card)
        {
            return LockList.Contains(card);
        }

        public void AddToLockList(Card card)
        {
            LockList.Add(card);
        }

        public void RemoveFromLockList(Card card)
        {
            var newList = new List<Card>(LockList);
            newList.ForEach(c =>
            {
                if (c.Uuid == card.Uuid)
                {
                    LockList.Remove(c);
                }
            });
        }
    }
}