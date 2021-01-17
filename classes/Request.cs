using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;

namespace mtcg
{
    public class Request : IRequest
    {
        private Url _url = new Url(string.Empty);
        private string _method = string.Empty;
        private string _userAgent = string.Empty;
        private string _contentType = string.Empty;
        private int _contentSize;
        private string _contentString;
        private byte[] _contentBytes;
        private Stream _contentStream;
        
        public Request(string request, int contentSize)
        {
            _contentSize = contentSize;
            ManageRequest(request);
        }

        private static readonly string[] Methods = {"GET", "PUT", "PATCH", "POST", "DELETE"};

        public bool IsValid { get; private set; }
        public string Method => _method;
        public IUrl Url => _url;
        public IDictionary<string, string> Headers { get; private set; } = new Dictionary<string, string>();

        public string UserAgent => _userAgent;

        public int HeaderCount => Headers?.Count ?? 0;

        public int ContentLength => _contentSize;
        public string ContentType => _contentType;
        public Stream ContentStream => _contentStream;
        public string ContentString => _contentString;
        public byte[] ContentBytes => _contentBytes;


        private bool IsRequestValid(IEnumerable<string> methods, IReadOnlyList<string> line)
        {
            //LINQ-Expression (string[] methods = datasource)
            //Enumerable.Any = determines whether any element of a sequence exists or satisfies a condition
            return methods.Any(method => line.Count == 3 && line[0].Contains(method));
        }

        private void GetContents(string[] content)
        {
            if (content.Length <= 0) return;
            _contentString = string.Join(Environment.NewLine, content);
            _contentBytes = Encoding.UTF8.GetBytes(ContentString);
            _contentStream = new MemoryStream(ContentBytes);
        }

        private void ManageRequest(string request)
        {
            Console.WriteLine(request);
            var reqInLines = request.Split(Environment.NewLine);                 //Split request into lines
            var firstLine = reqInLines[0].Split(' ', 3);          //first line with method & URL
            var contentIndex = Array.IndexOf(reqInLines, string.Empty);       //index where header ends
            var content = new List<string>();                                           //list for body-content
            var headerDict = new Dictionary<string, string>();                          //dictionary for header

            for (var i = 1; i < reqInLines.Length; i++)
            {
                //getting Key-Value Pairs for each line with ":"
                var trimReq = reqInLines[i].Replace(" ", "");
                var keyValue = trimReq.Split(":", StringSplitOptions.RemoveEmptyEntries);

                //skipping first line, end of header
                if (i < contentIndex - 1 && i > 0) headerDict.Add(keyValue[0].ToLower(), keyValue[1]);
                if (i > contentIndex) content.Add(trimReq);
                if (reqInLines[i].Contains("User-Agent")) _userAgent = keyValue[1];
                if (reqInLines[i].Contains("Content-Type")) _contentType = keyValue[1];
            }
            
            IsValid = IsRequestValid(Methods, firstLine);
            _method = firstLine[0].ToUpper();
            _url = new Url(firstLine[1]);
            GetContents(content.ToArray());
            Headers = headerDict;
        }
    }
}