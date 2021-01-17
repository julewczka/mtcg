using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BIF.SWE1.Interfaces;

namespace mtcg
{
    public class Url : IUrl
    {
        public Url(string url)
        {
            SplitUrl(url);
        }

        public string RawUrl { get; private set; }

        public string Path { get; private set; }

        //name=wei
        public IDictionary<string, string> Parameter { get; private set; } = new Dictionary<string, string>();

        public int ParameterCount { get; private set; }

        // '/'
        public string[] Segments { get; private set; } = Array.Empty<string>();

        //index.html
        public string FileName { get; private set; } = string.Empty;

        //.html
        public string Extension { get; private set; } = string.Empty;

        //#
        public string Fragment { get; private set; } = string.Empty;


        private string GetFragments(string url)
        {
            if (!url.Contains('#')) return string.Empty;
            var splitFragments = url.Split('#', 2, StringSplitOptions.RemoveEmptyEntries);
            RawUrl = splitFragments[0];
            var fragment = splitFragments[1];
            return fragment;
        }

        private string[] GetSegments(string url)
        {
            var urlSplit = url.Split('?', 2, StringSplitOptions.RemoveEmptyEntries).ToList();
            var segments = url.Contains('?')
                ? urlSplit[0].Split('/', StringSplitOptions.RemoveEmptyEntries).ToList()
                : url.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();

            if (segments.Count == 0) segments.Add("/");
            
            return segments.ToArray();
        }

        private StringBuilder GetPath(string[] segments)
        {
            var path = new StringBuilder();
            if (Segments.Length <= 0) return path.Append('/');

            foreach (var segment in segments)
            {
                path.Append('/');
                path.Append(segment);
            }

            return path;
        }

        private Dictionary<string, string> GetParameters(string url)
        {
            if (!url.Contains('?')) return new Dictionary<string, string>();
            var urlSplit = url.Split('?', 2, StringSplitOptions.RemoveEmptyEntries);
            var parameters = urlSplit[1].Split('&', StringSplitOptions.RemoveEmptyEntries);
            var paramsInDict = new Dictionary<string, string>();
            foreach (var parameter in parameters)
            {
                ParameterCount++;
                var keyValue = parameter.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
                paramsInDict.Add(keyValue[0], keyValue[1]);
            }

            return paramsInDict;
        }

        private string GetFileName(string[] segments)
        {
            var regEx = new Regex(@".+\..+");
            var filename = "";
            if (segments.Length <= 0) return "";
            if (regEx.IsMatch(segments[^1])) filename += segments[^1];
            return filename;
        }

        private string GetExtension(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return "";
            var splitFileName = filename.Split('.', StringSplitOptions.RemoveEmptyEntries);
            return "." + splitFileName.Last();
        }

        private void SplitUrl(string url)
        {
            RawUrl = url;
            Fragment = GetFragments(url);
            Segments = GetSegments(url);
            Path = GetPath(Segments).ToString();
            Parameter = GetParameters(url);
            FileName = GetFileName(Segments);
            Extension = GetExtension(FileName);
        }
    }
}