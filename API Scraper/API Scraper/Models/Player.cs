namespace API_Scraper.Models
{
    public class Player : BaseDataObject
    {
        public int Elo { get; set; }
        public string GamerTag { get; set; }
        public string Region { get; set; }
        public string MainCharacter { get; set; }

        public Player(string id, string gamerTag, int elo = 0, string region = "", string mainCharacter = "")
        {
            Id = id;
            Elo = elo;
            GamerTag = gamerTag;
            Region = region;
            MainCharacter = mainCharacter;
        }
    }
}
