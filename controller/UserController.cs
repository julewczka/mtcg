using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using mtcg.classes.entities;
using mtcg.repositories;

namespace mtcg.controller
{
    public class UserController
    {
        private readonly UserRepository _userRepo;
        public UserController()
        {
            _userRepo = new UserRepository();
        }
        public Response Put(User user, string username, string payload)
        {
            if (!user.Token.Contains(username) && user.Token != "admin-mtcgToken") return RTypes.Forbidden;

            var updateUser = JsonSerializer.Deserialize<User>(payload);
            if (updateUser == null) return RTypes.BadRequest;
            updateUser.Username = username;

            return _userRepo.Update(updateUser)
                ? RTypes.Created
                : RTypes.BadRequest;
        }

        public Response Post(string payload)
        {
            var createUser = JsonSerializer.Deserialize<User>(payload);
            if (createUser == null) return RTypes.BadRequest;
            createUser.Name = createUser.Username;
            createUser.Token = createUser.Username + "-mtcgToken";

            return _userRepo.Insert(createUser)
                ? RTypes.Created
                : RTypes.BadRequest;
        }

        public Response Get(User user, IReadOnlyList<string> resource)
        {
            if (resource.Count > 1 && !user.Token.Contains(resource[1])) return RTypes.Forbidden;

            var deckRepo = new DeckRepository();
            var fetchedUsers = new List<User>();
            var content = new StringBuilder();

            switch (resource.Count)
            {
                case 1:
                    if (user.Token != "admin-mtcgToken") return RTypes.Forbidden;
                    fetchedUsers.AddRange(_userRepo.GetAll());
                    fetchedUsers.ForEach(user =>
                        {
                            user.Deck = deckRepo.GetDeckByUserUuid(user.Id);
                            content.Append(JsonSerializer.Serialize(user) + "," + Environment.NewLine);
                        }
                    );
                    break;
                case 2:
                    var fetchedSingleUser = _userRepo.GetByUsername(resource[1]);
                    if (fetchedSingleUser == null) return RTypes.NotFoundRequest;
                    content.Append(JsonSerializer.Serialize(fetchedSingleUser) + "," + Environment.NewLine);
                    break;
                default:
                    return RTypes.BadRequest;
            }

            return RTypes.CResponse(content.ToString(), 200, "application/json");
        }

        public Response Delete(User user, string uuid)
        {
            if (user.Token != "admin-mtcgToken") return RTypes.Forbidden;
            var deleteUser = _userRepo.GetByUuid(uuid);
            return _userRepo.Delete(deleteUser)
                ? RTypes.HttpOk
                : RTypes.BadRequest;
        }
    }
}