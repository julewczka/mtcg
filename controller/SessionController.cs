using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class SessionController
    {
        /**
         * List for every logged-in user
         */
        private static readonly SortedDictionary<string, DateTime> SessionList = new();

        //TODO: implement GET-Method
        public static Response Login(string payload)
        {
            var loginUser = JsonSerializer.Deserialize<User>(payload);
            var timestamp = DateTime.Now;

            if (string.IsNullOrEmpty(loginUser?.Username)) return ResponseTypes.Unauthorized;

            if (CheckSessionList(loginUser.Username)) return ResponseTypes.MethodNotAllowed;

            var retrievedUser = SessionRepository.GetUserByName(loginUser.Username);
            if (retrievedUser == null || loginUser.Password != retrievedUser.Password)
                return ResponseTypes.Unauthorized;
            
            loginUser.Password = string.Empty;
            retrievedUser.Password = string.Empty;
            
            SessionList.Add(loginUser.Username, timestamp);
            SessionRepository.LogLogin(loginUser.Username, timestamp);

            return new Response("Authenticated") {ContentType = "text/plain", StatusCode = 200};
        }

        /**
         * remove User 2 hours after login
         */
        private static void CleanSessionList()
        {
            var now = DateTime.Now;
            foreach (var session in from session in SessionList
                let interval = now - session.Value
                where interval.Hours > 2
                select session)
            {
                SessionList.Remove(session.Key);
            }
        }

        private static bool CheckSessionList(string username)
        {
            CleanSessionList();
            return SessionList.ContainsKey(username);
        }

        public static SortedDictionary<string, DateTime> GetSessionList()
        {
            return SessionList;
        }
    }
}