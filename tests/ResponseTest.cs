using System;
using Xunit;

namespace mtcg
{
    public class ResponseTest
    {
        [Fact]
        public void IsResponseCorrect()
        {
            var response = new Response("OK") {StatusCode = 200, ContentType = "application/json"};
            Assert.Equal(200, response.StatusCode);
        }
        
        /// <summary>
        /// Check what happens if Headers["content-length"] is null
        /// </summary>
        [Fact]
        public void ContentLengthIsNull()
        {
            var response = new Response("OK") {StatusCode = 200, ContentType = "application/json"};
            response.Headers["content-length"] = null;
            Assert.Equal(0, response.ContentLength);
        }

        [Fact]
        public void ContentTypeIsNull()
        {
            var response = new Response("OK") {StatusCode = 200};
            Assert.Equal("text/plain", response.ContentType);
        }
        
    }
}