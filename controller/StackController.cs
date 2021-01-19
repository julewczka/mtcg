using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class StackController
    {
        public static Response Get(string token)
        {
            var response = new Response() {ContentType = "application/json"};
            var listStack = new List<Card>();
            var data = new StringBuilder();

            var user = UserRepository.SelectUserByToken(token);

            if (user == null) return ResponseTypes.NotFoundRequest;
           
            listStack = StackRepository.GetStack(user.Id);
            listStack.ForEach(card => { data.Append(JsonSerializer.Serialize(card)); });

            response.StatusCode = 200;
            response.SetContent(data.ToString());

            return response;
        }
    }
}