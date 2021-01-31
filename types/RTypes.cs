namespace mtcg.controller
{
    public static class RTypes
    {
        public static readonly Response HttpOk = new("OK")
            {StatusCode = 200, ContentType = "text/plain"};

        public static readonly Response Created = new("Created")
            {StatusCode = 201, ContentType = "text/plain"};
        
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

        public static Response CError(string content, int code)
        {
            return new(content) {StatusCode = code, ContentType = "text/plain"};
        }

        public static Response CResponse(string content, int code, string contentType)
        {
            return new(content) {StatusCode = code, ContentType = contentType};

        }
    }
}