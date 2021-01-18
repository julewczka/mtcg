using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class UserController
    {
        public static Response Put(string username, string payload)
        {
            var updateUser = JsonSerializer.Deserialize<User>(payload);
            if (updateUser == null) return ResponseTypes.BadRequest;
            updateUser.Username = username;

            return UserRepository.UpdateUser(updateUser)
                ? new Response("Updated") {ContentType = "text/plain", StatusCode = 201}
                : ResponseTypes.BadRequest;
        }

        public static Response Post(string payload)
        {
            var createUser = JsonSerializer.Deserialize<User>(payload);
            if (createUser == null) return ResponseTypes.BadRequest;
            createUser.Name = createUser.Username;
            createUser.Token = createUser.Username + "-mtcgToken";

            return UserRepository.InsertUser(createUser)
                ? new Response("Created") {ContentType = "text/plain", StatusCode = 201}
                : ResponseTypes.BadRequest;
        }

        public static Response Get(IReadOnlyList<string> resource)
        {
            var response = new Response() {ContentType = "application/json"};
            var fetchedUsers = new List<User>();
            var data = new StringBuilder();

            try
            {
                switch (resource.Count)
                {
                    case 1:
                        fetchedUsers.AddRange(UserRepository.SelectAll());
                        fetchedUsers.ForEach(user =>
                        {
                            var fetchedUser = JsonSerializer.Serialize(user);
                            data.Append(fetchedUser + "," + Environment.NewLine);
                        });

                        response.StatusCode = 200;
                        response.SetContent(data.ToString());
                        break;
                    case 2:
                        var fetchedSingleUser = UserRepository.SelectUser(resource[1]);
                        data.Append(JsonSerializer.Serialize(fetchedSingleUser) + "," + Environment.NewLine);

                        response.StatusCode = 200;
                        response.SetContent(data.ToString());
                        break;
                    default:
                        return ResponseTypes.BadRequest;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return ResponseTypes.BadRequest;
            }

            return response;
        }

        public static Response Delete(string uuid)
        {
            return UserRepository.DeleteUser(uuid)
                ? new Response("OK") {ContentType = "text/plain", StatusCode = 200}
                : ResponseTypes.BadRequest;
        }
    }
}