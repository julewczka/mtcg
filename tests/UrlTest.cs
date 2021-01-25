using System.Collections.Generic;
using Xunit;

namespace mtcg
{
    public class UrlTest
    {
        [Fact]
        public void SplitSegments()
        {
            var urlString = "/users/julewczka";
            var url = new Url(urlString);
            
            Assert.Equal("users", url.Segments[0]);
        }
        
        [Fact]
        public void GetFragments()
        {
            var urlString = "/users/julewczka?id=5&name=wei";
            var url = new Url(urlString);
            
            Assert.Equal("", url.Fragment);
        }
        
        [Fact]
        public void GetParameters()
        {
            var urlString = "/users/julewczka?id=5&name=wei";
            var url = new Url(urlString);
            var parameters = new Dictionary<string, string>();
            parameters.Add("id", "5");
            
            Assert.Equal("5", url.Parameter["id"]);
        }
        
    }
}