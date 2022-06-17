namespace API_Scraper.Models
{
    public class Set : BaseDataObject
    {
        public int TournamentId { get; set; }
        public System.DateTime Date { get; set; }
        
        public string DisplayScore { get; set; }
        public int WinnerId { get; set; }
        public int LoserId { get; set; }

    }
}
