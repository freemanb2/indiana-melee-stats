namespace API_Scraper.API
{
    public class Standing
    {
        public bool IsFinal { get; set; }
        public object Metadata { get; set; }
        public int Placement { get; set; }
        public Entrant Entrant { get; set; }
        
    }
}