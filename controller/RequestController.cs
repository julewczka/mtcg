using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BIF.SWE1.Interfaces;
using mtcg.classes.entities;
using mtcg.repositories;
using Npgsql;

namespace mtcg.controller
{
    public static class RequestController
    {
        public static Response HandleRequest(Request request, string payload)
        {
            var protocol = request.Method;
            var resource = request.Url.Segments;
            var token = "";
            if (request.Headers.ContainsKey("authorization"))
            {
                var authHeader = request.Headers["authorization"];
                Console.WriteLine($"Test1: {authHeader}");
                if (authHeader.Length > 5)
                {
                    Console.WriteLine($" 2: {authHeader}");
                    token = authHeader.Substring(5);
                }
            }

            var response = new Response();
            try
            {
                response = protocol switch
                {
                    "GET" => Get(token, resource),
                    "POST" => Post(token, resource, payload),
                    "PUT" => Put(token, resource, payload),
                    "DELETE" => Delete(token, resource),
                    _ => ResponseTypes.BadRequest
                };
            }
            catch (PostgresException pe)
            {
                Console.WriteLine(pe.Message);
                Console.WriteLine(pe.StackTrace);
            }
            catch (JsonException je)
            {
                Console.WriteLine(je.Message);
                Console.WriteLine(je.StackTrace);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            return response;
        }

        private static Response Get(string token, IReadOnlyList<string> resource)
        {
            if (!IsUserAuthorized(token)) return ResponseTypes.Unauthorized;

            return resource[0] switch
            {
                "/" => ResponseTypes.CustomResponse("<h1>Welcome to the Monster Trading Card Game!</h1>", 200,
                    "text/html"),
                "users" => UserController.Get(token, resource),
                "sessions" => SessionController.GetLogs(),
                "packages" => PackageController.Get(resource),
                "stack" => StackController.Get(token),
                "deck" => DeckController.GetDeckByToken(token),
                "cards" => CardController.Get(token),
                "tradings" => TradingController.Get(resource),
                "stats" => StatsController.Get(token),
                "score" => ScoreController.GetScore(),
                _ => ResponseTypes.NotFoundRequest
            };
        }

        private static Response Post(string token, IReadOnlyList<string> resource, string payload)
        {
            if (resource[0] != "sessions")
            {
                if (!IsUserAuthorized(token))
                    return ResponseTypes.CustomError($"Fail: {token}", 403); //ResponseTypes.Unauthorized;
            }

            if (!IsValidJson(resource[0], payload)) return ResponseTypes.BadRequest;
            return resource[0] switch
            {
                "users" => UserController.Post(payload),
                "sessions" => SessionController.Login(payload),
                "packages" => PackageController.Post(token, payload),
                "stack" => ResponseTypes.MethodNotAllowed,
                "deck" => DeckController.CreateDeck(token, payload),
                "tradings" => TradingController.Post(token, resource, payload),
                "transactions" => TransactionController.StartTransaction(resource[1], token),
                "battles" => BattleController.Post(token),
                "cards" => ResponseTypes.MethodNotAllowed, //CardController.Post(payload),
                "/" => ResponseTypes.MethodNotAllowed,
                _ => ResponseTypes.NotFoundRequest
            };
        }

        private static Response Put(string token, IReadOnlyList<string> resource, string payload)
        {
            if (!IsUserAuthorized(token)) return ResponseTypes.Unauthorized;
            if (resource.Count < 2 && !IsValidJson(resource[0], payload)) return ResponseTypes.BadRequest;

            return resource[0] switch
            {
                "users" => UserController.Put(token, resource[1], payload),
                "deck" => DeckController.ConfigureDeck(token, payload),
                "cards" => ResponseTypes.MethodNotAllowed, //CardController.Put(resource[1], payload),
                "sessions" => ResponseTypes.MethodNotAllowed,
                "/" => ResponseTypes.MethodNotAllowed,
                _ => ResponseTypes.NotFoundRequest
            };
        }

        private static Response Delete(string token, IReadOnlyList<string> resource)
        {
            if (!IsUserAuthorized(token)) return ResponseTypes.Unauthorized;
            if (resource.Count < 2) return ResponseTypes.BadRequest;
            return resource[0] switch
            {
                "users" => UserController.Delete(token, resource[1]),
                "cards" => ResponseTypes.MethodNotAllowed, //CardController.Delete(resource[1]),
                "sessions" => ResponseTypes.MethodNotAllowed,
                "tradings" => TradingController.Delete(token, resource[1]),
                "/" => ResponseTypes.MethodNotAllowed,
                _ => ResponseTypes.NotFoundRequest
            };
        }

        private static bool IsValidJson(string resource, string json)
        {
            Console.WriteLine($"JSON:{json}");
            try
            {
                switch (resource)
                {
                    case "users":
                        JsonSerializer.Deserialize<User>(json);
                        break;
                    case "packages":
                        JsonSerializer.Deserialize<Card[]>(json);
                        break;
                    case "cards":
                        JsonSerializer.Deserialize<Card>(json);
                        break;
                    case "tradings":
                        JsonSerializer.Deserialize<Trading>(json);
                        break;
                }
            }
            catch (JsonException)
            {
                Console.WriteLine("invalid JSON!");
                return false;
            }

            return true;
        }

        private static bool IsUserAuthorized(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;
            var user = UserRepository.SelectUserByToken(token);
            return (user != null && SessionController.CheckSessionList(user.Username));
        }
    }
}