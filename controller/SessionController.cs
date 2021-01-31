using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using mtcg.classes.entities;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class SessionController
    {
        private static readonly object SessionLock = new object();

        /**
         * List for every logged-in user
         */
        private static readonly SortedDictionary<string, DateTime> SessionList = new();

        /**
         * compare received user credentials with database records
         * add to SessionList after successful comparison
         */
        public static Response Login(string payload)
        {
            lock (SessionLock)
            {
                var loginUser = JsonSerializer.Deserialize<User>(payload);
                var timestamp = DateTime.Now;

                if (string.IsNullOrEmpty(loginUser?.Username)) return RTypes.Unauthorized;

                if (CheckSessionList(loginUser.Username)) return RTypes.MethodNotAllowed;

                var retrievedUser = SessionRepository.GetUserByName(loginUser.Username);
                if (retrievedUser == null || loginUser.Password != retrievedUser.Password)
                    return RTypes.Unauthorized;

                loginUser.Password = string.Empty;
                retrievedUser.Password = string.Empty;

                SessionList.Add(loginUser.Username, timestamp);
                SessionRepository.LogLogin(loginUser.Username, timestamp);
            }
            return RTypes.CResponse("Authenticated", 200, "text/plain");
        }

        public static Response GetLogs()
        {
            var logs = SessionRepository.GetLogs();
            var data = new StringBuilder();
            if (logs == null) return RTypes.NotFoundRequest;

            logs.ForEach(log => { data.Append(JsonSerializer.Serialize(log) + "," + Environment.NewLine); }
            );

            return RTypes.CResponse(data.ToString(), 200, "application/json");
        }

        /**
         * logout User 2 hours after login
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

        /**
         * checks if user is logged in
         */
        public static bool CheckSessionList(string username)
        {
            var name = string.IsNullOrEmpty(username)
                ? "empty"
                : username;

            CleanSessionList();
            return SessionList.ContainsKey(name);
        }
    }
}