using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
//using BIF.SWE1.Interfaces;
using mtcg.controller;

namespace mtcg
{
    public class Server
    {
        private const int Port = 12345;

        public static void Main(string[] args)
        {
            Listen();
        }


        /** 
         * listens on PORT 12345
         */
        private static void Listen()
        {
            var listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Console.Write($"Server started at port {Port}\n");

            while (true)
            {
                var clientSocket = listener.AcceptSocket();
                var connection = new Thread(() => HandleRequest(clientSocket));
                connection.Start();
            }
        }

        /**
         * handle's the request and sends the response
         */
        private static void HandleRequest(Socket socket)
        {
            using var stream = new NetworkStream(socket);
            using var sr = new StreamReader(stream);
            var rawRequest = new StringBuilder();
            var contentSize = 0;

            //get header
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();

                if (line != null && line.Contains("Content-Length"))
                {
                    var trimLine = line.Replace(" ", "").Split(":");
                    contentSize = int.Parse(trimLine[1]);
                }

                rawRequest.Append(line + "\n");

                if (string.IsNullOrEmpty(line)) break;
            }

            var content = ReadRequestContent(sr, contentSize);
            rawRequest.Append(content);
            var request = new Request(rawRequest.ToString(), contentSize);
            if (request.IsValid)
            {
                var response = RequestController.HandleRequest(request, content.ToString());
                response.Send(stream);
            }
            else
            {
                var response = ResponseTypes.BadRequest;
                response.Send(stream);
            }

            socket.Close();
        }

        private static StringBuilder ReadRequestContent(TextReader stream, int contentSize)
        {
            if (contentSize == 0) return new StringBuilder();
            var content = new StringBuilder();
            var lines = new char[contentSize];
            stream.Read(lines, 0, contentSize);
            Array.ForEach(lines, line => content.Append(line));
            return content;
        }
    }
}