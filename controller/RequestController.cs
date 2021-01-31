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
                if (authHeader.Length > 5)
                {
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
                    _ => RTypes.BadRequest
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
            if (!IsUserAuthorized(token)) return RTypes.Unauthorized;

            return resource[0] switch
            {
                "/" => RTypes.CResponse("<h1>Welcome to the Monster Trading Card Game!</h1>", 200,
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
                _ => RTypes.NotFoundRequest
            };
        }

        private static Response Post(string token, IReadOnlyList<string> resource, string payload)
        {
            if (resource[0] != "sessions" && resource[0] != "users")
            {
                if (!IsUserAuthorized(token))
                    return  RTypes.Unauthorized;
            }

            if (!IsValidJson(resource[0], payload)) return RTypes.BadRequest;
            return resource[0] switch
            {
                "users" => UserController.Post(payload),
                "sessions" => SessionController.Login(payload),
                "packages" => PackageController.Post(token, payload),
                "stack" => RTypes.MethodNotAllowed,
                "deck" => DeckController.CreateDeck(token, payload),
                "tradings" => TradingController.Post(token, resource, payload),
                "transactions" => TransactionController.StartTransaction(resource[1], token),
                "battles" => BattleController.Post(token),
                "cards" => RTypes.MethodNotAllowed, //CardController.Post(payload),
                "/" => RTypes.MethodNotAllowed,
                _ => RTypes.NotFoundRequest
            };
        }

        private static Response Put(string token, IReadOnlyList<string> resource, string payload)
        {
            if (!IsUserAuthorized(token)) return RTypes.Unauthorized;
            if (resource.Count < 2 && !IsValidJson(resource[0], payload)) return RTypes.BadRequest;

            return resource[0] switch
            {
                "users" => UserController.Put(token, resource[1], payload),
                "deck" => DeckController.ConfigureDeck(token, payload),
                "cards" => RTypes.MethodNotAllowed, //CardController.Put(resource[1], payload),
                "sessions" => RTypes.MethodNotAllowed,
                "/" => RTypes.MethodNotAllowed,
                _ => RTypes.NotFoundRequest
            };
        }

        private static Response Delete(string token, IReadOnlyList<string> resource)
        {
            if (!IsUserAuthorized(token)) return RTypes.Unauthorized;
            if (resource.Count < 2) return RTypes.BadRequest;
            return resource[0] switch
            {
                "users" => UserController.Delete(token, resource[1]),
                "cards" => RTypes.MethodNotAllowed, //CardController.Delete(resource[1]),
                "sessions" => RTypes.MethodNotAllowed,
                //"tradings" => TradingController.Delete(token, resource[1]),
                "/" => RTypes.MethodNotAllowed,
                _ => RTypes.NotFoundRequest
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
                        //JsonSerializer.Deserialize<Trading>(json);
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