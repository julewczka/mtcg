namespace mtcg.controller
{
    public static class ResponseTypes
    {
        public static readonly Response BadRequest = new("Bad Request")
            {StatusCode = 400, ContentType = "text/plain"};

        public static readonly Response Unauthorized = new("Unauthorized")
            {StatusCode = 401, ContentType = "text/plain"};

        public static readonly Response Forbidden = new("Forbidden")
            {StatusCode = 403, ContentType = "text/plain"};
        
        public static readonly Response NotFoundRequest = new("Not found")
            {StatusCode = 404, ContentType = "text/plain"};
        
        public static readonly Response MethodNotAllowed = new("Method not Allowed")
            {StatusCode = 405, ContentType = "text/plain"};
    }
}