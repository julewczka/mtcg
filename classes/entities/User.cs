using BIF.SWE1.Interfaces;

namespace mtcg.classes.entities
{
    public class User : IResource
    {

        public string Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Token { get; set; }
        
        public string Name { get; set; }
        public string Bio { get; set; } = string.Empty;

        public string Image { get; set; } = string.Empty;

        public double Coins { get; set; } = 20;

        public Deck Deck { get; set; }

        private Stack Stack { get; set; }
    }
}