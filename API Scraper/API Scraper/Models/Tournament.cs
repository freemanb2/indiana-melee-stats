namespace API_Scraper.Models
{
    public class Tournament : BaseDataObject
    {
        public string TournamentName { get; set; }
        public List<Event> Events { get; set; }

        public Tournament(API.Tournament API_Tournament)
        {
            TournamentName = API_Tournament.Name;
            Events = API_Tournament.Events;
        }
    }
}
