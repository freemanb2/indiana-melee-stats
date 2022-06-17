namespace API_Scraper.Models
{
    public class Player : BaseDataObject
    {
        public int Elo { get; set; }
        public string GamerTag { get; set; }
        public string Region { get; set; }
        public string MainCharacter { get; set; }
    }
}
