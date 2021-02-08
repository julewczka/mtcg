using System;
using System.Collections.Generic;
using System.Text.Json;
using mtcg.classes.entities;
using mtcg.repositories;
using Npgsql;

namespace mtcg.controller
{
    public class RequestController
    {
        private readonly UserRepository _userRepo;
        private readonly SessionController _sessionCtrl;
        public RequestController()
        {
            _userRepo = new UserRepository();
            _sessionCtrl = new SessionController();
        }
        
        public Response HandleRequest(Request request, string payload)
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

        private Response Get(string token, IReadOnlyList<string> resource)
        {
            if (string.IsNullOrEmpty(token)) return RTypes.BadRequest;
            var user = _userRepo.GetByToken(token);
            if (!IsUserAuthorized(user)) return RTypes.Unauthorized;

            return resource[0] switch
            {
                "/" => RTypes.CResponse("<h1>Welcome to the Monster Trading Card Game!</h1>", 200,
                    "text/html"),
                "users" => new UserController().Get(user, resource),
                "sessions" => new SessionController().GetLogs(),
                "packages" => new PackageController().Get(resource),
                "stack" => new StackController().Get(user),
                "deck" => new DeckController().GetDeckByUser(user),
                "cards" => new CardController().Get(user),
                "tradings" => new TradingController().Get(resource),
                "stats" => new StatsController().Get(user),
                "score" => new ScoreController().GetScore(),
                _ => RTypes.NotFoundRequest
            };
        }

        private Response Post(string token, IReadOnlyList<string> resource, string payload)
        {
            User user = null;
            
            if (!string.IsNullOrEmpty(token)) user = _userRepo.GetByToken(token);

            if (resource[0] != "sessions" && resource[0] != "users")
            {
                if (!IsUserAuthorized(user)) return RTypes.Unauthorized;
            }

            if (!IsValidJson(resource[0], payload)) return RTypes.BadRequest;
            return resource[0] switch
            {
                "users" => new UserController().Post(payload),
                "sessions" => new SessionController().Login(payload),
                "packages" => new PackageController().Post(token, payload),
                "stack" => RTypes.MethodNotAllowed,
                "deck" => new DeckController().CreateDeck(user, payload),
                "tradings" => new TradingController().Post(user, resource, payload),
                "transactions" => TransactionController.StartTransaction(resource[1], user),
                "battles" => new BattleController().Post(token),
                "cards" => new CardController().Post(payload),
                "/" => RTypes.MethodNotAllowed,
                _ => RTypes.NotFoundRequest
            };
        }

        private Response Put(string token, IReadOnlyList<string> resource, string payload)
        {
            if (string.IsNullOrEmpty(token)) return RTypes.BadRequest;
            var user = _userRepo.GetByToken(token);
            if (!IsUserAuthorized(user)) return RTypes.Unauthorized;
            
            if (resource.Count < 2 && !IsValidJson(resource[0], payload)) return RTypes.BadRequest;

            return resource[0] switch
            {
                "users" => new UserController().Put(user, resource[1], payload),
                "deck" => new DeckController().ConfigureDeck(user, payload),
                "cards" => new CardController().Put(resource[1], payload),
                "sessions" => RTypes.MethodNotAllowed,
                "/" => RTypes.MethodNotAllowed,
                _ => RTypes.NotFoundRequest
            };
        }

        private Response Delete(string token, IReadOnlyList<string> resource)
        {
            if (string.IsNullOrEmpty(token)) return RTypes.BadRequest;
            var user = _userRepo.GetByToken(token);
            if (!IsUserAuthorized(user)) return RTypes.Unauthorized;
            
            if (resource.Count < 2) return RTypes.BadRequest;
            return resource[0] switch
            {
                "users" => new UserController().Delete(user, resource[1]),
                "cards" => new CardController().Delete(resource[1]),
                "sessions" => RTypes.MethodNotAllowed,
                "tradings" => new TradingController().Delete(user, resource[1]),
                "/" => RTypes.MethodNotAllowed,
                _ => RTypes.NotFoundRequest
            };
        }

        private bool IsValidJson(string resource, string json)
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

        private bool IsUserAuthorized(User user)
        {
            return user?.Id != null && _sessionCtrl.CheckSessionList(user.Username);
        }
    }
}