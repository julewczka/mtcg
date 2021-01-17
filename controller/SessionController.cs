using System;
using System.Text.Json;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class SessionController
    {
        //TODO: implement GET-Method
        public static Response Post(string payload)
        {
            var response = new Response() {ContentType = "text/plain"};
            try
            {
                var loginUser = JsonSerializer.Deserialize<User>(payload);
  
                if (string.IsNullOrEmpty(loginUser?.Username)) return ResponseTypes.Unauthorized;
                var retrievedUser = SessionRepository.GetUserByName(loginUser.Username);
                
                if (loginUser.Password == retrievedUser.Password)
                {
                    UserRepository.UpdateUser(retrievedUser);
                    
                    response.StatusCode = 200;
                    response.SetContent("Authenticated");
                    return response;
                }
                
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                return ResponseTypes.BadRequest;
            }
            return ResponseTypes.Unauthorized;
        }
        
    }
}