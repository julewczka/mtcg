using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;

namespace mtcg
{
    public class Response : IResponse
    {
        private int _statusCode;
        private string _content = string.Empty;
        private string _contentType = string.Empty;

        public Response()
        {
            Headers = new Dictionary<string, string> {{"Server", "BIF-SWE1-Server"}};
        }

        public Response(string content)
        {
            Headers = new Dictionary<string, string> {{"Server", "BIF-SWE1-Server"}};
            SetContent(content);
        }

        public IDictionary<string, string> Headers { get; }
        public int ContentLength => Headers["content-length"] == null ? 0 : int.Parse(Headers["content-length"]);

        public string ContentType
        {
            get => string.IsNullOrEmpty(_contentType) ? "text/plain" : _contentType;
            set { value ??= "text/plain"; } 
        }

        public int StatusCode
        {
            get
            {
                if (_statusCode == 0) throw new Exception("Statuscode 0!");
                return _statusCode;
            }
            set
            {
                if (value >= 0 && value <= 511) _statusCode = value;
            }
        }

        public string Status => SetStatus(StatusCode);

        public void AddHeader(string header, string value)
        {
            if (Headers.ContainsKey(header)) Headers[header] = value;
            if (!Headers.ContainsKey(header)) Headers.Add(header, value);
        }

        public string ServerHeader
        {
            get => Headers["Server"];

            set => Headers["Server"] = (!string.IsNullOrEmpty(value)) ? value : throw new Exception("Server value is empty!");
        }
        
        /**
         * Set content
         */
        public void SetContent(string content)
        {
            _content = content;
            Headers["content-length"] = _content.Length.ToString();
        }
        
        /**
         * Content: Byte -> String
         */
        public void SetContent(byte[] content)
        {
            _content = Encoding.UTF8.GetString(content);
            Headers["content-length"] = _content.Length.ToString();
        }
        
        /**
         * Content: Stream -> String
         */
        public void SetContent(Stream stream)
        {
            var str = new StreamReader(stream);

            while (!str.EndOfStream)
            {
                _content += str.ReadLine();
            }
            Headers["content-length"] = _content.Length.ToString();
        }

        public void Send(Stream network) 
        {
            AddHeader("content-type", ContentType);
            var wr = new StreamWriter(network);
            var response = new StringBuilder();
            response.Append($"HTTP/1.1 {Status}{Environment.NewLine}");
            
            //content in header
            foreach (var (key, value) in Headers)
            {
                response.Append($"{key}: {value}{Environment.NewLine}");
            }

            response.Append(Environment.NewLine);
            
            //content in body
            foreach (var cont in _content)
            {
                response.Append(cont);
            }

            wr.Write(response);
            wr.Flush();
        }

        private static string SetStatus(int statusCode)
        {
            return statusCode switch
            {
                200 => "200 OK",
                201 => "201 CREATED",
                202 => "202 ACCEPTED",
                400 => "400 BAD REQUEST",
                401 => "401 UNAUTHORIZED",
                403 => "403 FORBIDDEN",
                404 => "404 NOT FOUND",
                405 => "405 METHOD NOT ALLOWED",
                500 => "500 INTERNAL SERVER ERROR",
                _ => "HTTP Error",
            };
        }
    }
}