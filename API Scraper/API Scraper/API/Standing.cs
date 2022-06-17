namespace API_Scraper.API
{
    public class Standing
    {
        public int Id { get; set; }
        public Entrant Entrant { get; set; }
        public bool IsFinal { get; set; }
        public object Metadata { get; set; }
        public int Placement { get; set; }
        public Player Player { get; set; }
        
    }
}