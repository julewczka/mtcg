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
            var response = new Response() {ContentType = "text/plain"};
            try
            {
                var updateUser = JsonSerializer.Deserialize<User>(payload);
                if (updateUser != null)
                {
                    updateUser.Username = username;
                    UserRepository.UpdateUser(updateUser);
                }

                response.StatusCode = 201;
                response.SetContent("Updated");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                return ResponseTypes.BadRequest;
            }

            return response;
        }

        public static Response Post(string payload)
        {
            var response = new Response() {ContentType = "text/plain"};
            try
            {
                var createUser = JsonSerializer.Deserialize<User>(payload);
                if (string.IsNullOrEmpty(createUser?.Username)) return ResponseTypes.BadRequest;
                createUser.Name = createUser.Username;
                createUser.Token = createUser.Username + "-mtcgToken";
                
                UserRepository.InsertUser(createUser);

                response.StatusCode = 201;
                response.SetContent("Created");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                return ResponseTypes.BadRequest;
            }

            return response;
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
                        fetchedUsers.ForEach(src =>
                        {
                            var fetchedUser = JsonSerializer.Serialize(src);
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
            var response = new Response() {ContentType = "text/plain"};
            try
            {
                UserRepository.DeleteUser(uuid);

                response.StatusCode = 200;
                response.SetContent("OK");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                return ResponseTypes.BadRequest;
            }

            return response;
        }
    }
}