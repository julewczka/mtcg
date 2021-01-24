namespace mtcg
{
    public class Trading
    {
        public string Uuid { get; set; }
        public string CardToTrade { get; set; }
        
        public string Trader { get; set; }
        public string CardType { get; set; }
        public double MinimumDamage { get; set; }
    }
}