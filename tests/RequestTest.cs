using System;
using Xunit;

namespace mtcg
{
    public class RequestTest
    {
        private readonly Request _request;

        public RequestTest()
        {
            var request = "POST /sessions HTTP/1.1 + " + Environment.NewLine +
                          "Content-Type: application/json" + Environment.NewLine +
                          "User-Agent: PostmanRuntime/7.26.8" + Environment.NewLine +
                          "Accept: */*" + Environment.NewLine +
                          "Postman-Token: f36ad491-a8f9-4392-9d75-86364652f6b3" + Environment.NewLine +
                          "Host: localhost:12345" + Environment.NewLine +
                          "Accept-Encoding: gzip, deflate, br" + Environment.NewLine +
                          "Connection: keep-alive" + Environment.NewLine +
                          "Content-Length: 60";


            _request = new Request(request, 0);
        }

        /// <summary>
        /// Test if Request get the correct protocol
        /// </summary>
        [Fact]
        public void GetMethodOfRequest()
        {
            Assert.Equal("POST", _request.Method);
        }

        /// <summary>
        /// Test if incorrect requests are expecting well.
        /// </summary>
        [Fact]
        public void ReturnFalseOnInvalidProtocol()
        {
            var invalidProtocol = "FAIL /sessions HTTP/1.1 + " + Environment.NewLine +
                              "Content-Type: application/json" + Environment.NewLine +
                              "User-Agent: PostmanRuntime/7.26.8" + Environment.NewLine +
                              "Accept: */*" + Environment.NewLine +
                              "Postman-Token: f36ad491-a8f9-4392-9d75-86364652f6b3" + Environment.NewLine +
                              "Host: localhost:12345" + Environment.NewLine +
                              "Accept-Encoding: gzip, deflate, br" + Environment.NewLine +
                              "Connection: keep-alive" + Environment.NewLine +
                              "Content-Length: 60";
            var request = new Request(invalidProtocol, 0);
            Assert.False(request.IsValid);
        }

        /// <summary>
        /// Test parsing the content type
        /// </summary>
        [Fact]
        public void ContentTypeShouldBeApplicationJson()
        {
            Assert.Equal("application/json", _request.ContentType);
        }
    }
}