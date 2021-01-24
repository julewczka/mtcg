using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using mtcg.classes.entities;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class UserController
    {
        public static Response Put(string token, string username, string payload)
        {
            if (token != "admin-mtcgToken" || !token.Contains(username)) return ResponseTypes.Forbidden;

            var updateUser = JsonSerializer.Deserialize<User>(payload);
            if (updateUser == null) return ResponseTypes.BadRequest;
            updateUser.Username = username;

            return UserRepository.UpdateUser(updateUser)
                ? ResponseTypes.Created
                : ResponseTypes.BadRequest;
        }

        public static Response Post(string payload)
        {
            var createUser = JsonSerializer.Deserialize<User>(payload);
            if (createUser == null) return ResponseTypes.BadRequest;
            createUser.Name = createUser.Username;
            createUser.Token = createUser.Username + "-mtcgToken";

            return UserRepository.InsertUser(createUser)
                ? ResponseTypes.Created
                : ResponseTypes.BadRequest;
        }

        public static Response Get(string token, IReadOnlyList<string> resource)
        {
            if (resource.Count > 1 && !token.Contains(resource[1])) return ResponseTypes.Forbidden;
            
            var fetchedUsers = new List<User>();
            var content = new StringBuilder();

            switch (resource.Count)
            {
                case 1:
                    if (token != "admin-mtcgToken") return ResponseTypes.Forbidden;
                    fetchedUsers.AddRange(UserRepository.SelectAll());
                    fetchedUsers.ForEach(user =>
                        {
                            user.Deck = DeckRepository.GetDeckByUserUuid(user.Id);
                            content.Append(JsonSerializer.Serialize(user) + "," + Environment.NewLine);
                        }
                    );
                    break;
                case 2:
                    var fetchedSingleUser = UserRepository.SelectUserByUsername(resource[1]);
                    if (fetchedSingleUser == null) return ResponseTypes.NotFoundRequest;
                    content.Append(JsonSerializer.Serialize(fetchedSingleUser) + "," + Environment.NewLine);
                    break;
                default:
                    return ResponseTypes.BadRequest;
            }

            return ResponseTypes.CustomResponse(content.ToString(), 200, "application/json");
        }

        public static Response Delete(string token, string uuid)
        {
            if (token != "admin-mtcgToken") return ResponseTypes.Forbidden;

            return UserRepository.DeleteUser(uuid)
                ? ResponseTypes.HttpOk
                : ResponseTypes.BadRequest;
        }
    }
}