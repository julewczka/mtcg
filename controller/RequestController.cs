using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BIF.SWE1.Interfaces;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class RequestController
    {
        public static Response HandleRequest(string protocol, string[] resource, string payload)
        {
            return protocol switch
            {
                "GET" => Get(resource),
                "POST" => Post(resource[0], payload),
                "PUT" => Put(resource, payload),
                "DELETE" => Delete(resource),
                _ => ResponseTypes.BadRequest
            };
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
                "sessions" => SessionController.Post(payload),
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
         * TODO: only works for User-Class atm!
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