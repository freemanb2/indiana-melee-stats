namespace API_Scraper.API
{
    public class Player
    {
        public int Id { get; set; }
        public string GamerTag { get; set; }
        public string Prefix { get; set; }
        public SetConnection Sets { get; set; }
        public User User { get; set; }
    }
}
