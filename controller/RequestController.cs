using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BIF.SWE1.Interfaces;
using mtcg.repositories;
using Npgsql;

namespace mtcg.controller
{
    public static class RequestController
    {
        //TODO: Catch all exceptions
        public static Response HandleRequest(string protocol, string[] resource, string payload)
        {
            var response = new Response();
            try
            {
                response = protocol switch
                {
                    "GET" => Get(resource),
                    "POST" => Post(resource[0], payload),
                    "PUT" => Put(resource, payload),
                    "DELETE" => Delete(resource),
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

        /**
         * forms & returns a response
         * request data from resources (e.g. /users)
         * param -> needs a string array of segments
         */
        private static Response Get(IReadOnlyList<string> resource)
        {
            var response = new Response("<h1>Welcome to the Monster Trading Card Game!</h1>")
                {StatusCode = 200, ContentType = "text/html"};
            
            return resource[0] switch
            {
                "/" => response,
                "users" => UserController.Get(resource),
                "sessions" => ResponseTypes.MethodNotAllowed,
                "cards" => CardController.Get(resource),
                _ => ResponseTypes.NotFoundRequest
            };
        }

        /**
         * forms & returns a response
         * insert data to resources (e.g. /users)
         * param resource: resource (e.g. /users)
         * param payload: body in JSON-format (e.g. username, token)
         */
        private static Response Post(string resource, string payload)
        {
            if (!IsValidJson(resource, payload)) return ResponseTypes.BadRequest;
            //TODO: Session for Login
            return resource switch
            {
                "users" => UserController.Post(payload),
                "sessions" => SessionController.Login(payload),
                "packages" => PackageController.Post(payload),
                "/" => ResponseTypes.MethodNotAllowed,
                _ => ResponseTypes.NotFoundRequest
            };
        }

        //TODO: Login first
        private static Response Put(IReadOnlyList<string> resource, string payload)
        {

            if (resource.Count < 2 && !IsValidJson(resource[0],payload)) return ResponseTypes.BadRequest;

            return resource[0] switch
            {
                "users" => UserController.Put(resource[1], payload),
                "cards" => CardController.Put(resource[1], payload),
                _ => ResponseTypes.NotFoundRequest
            };
        }
        
        private static Response Delete(IReadOnlyList<string> resource)
        {
            if (resource.Count < 2) return ResponseTypes.BadRequest;
            return resource[0] switch
            {
                "users" => UserController.Delete(resource[1]),
                "/" => ResponseTypes.MethodNotAllowed,
                _ => ResponseTypes.NotFoundRequest
            };
        }

        /**
         * check if string is valid JSON
         */
        private static bool IsValidJson(string resource, string json)
        {
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
                }
                
            }
            catch (JsonException)
            {
                Console.WriteLine("invalid JSON!");
                return false;
            }

            return true;
        }
    }
}