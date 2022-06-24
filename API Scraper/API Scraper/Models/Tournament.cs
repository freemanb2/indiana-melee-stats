using System.Collections.Generic;

namespace API_Scraper.Models
{
    public class Tournament : BaseDataObject
    {
        public string TournamentName { get; set; }
        public List<Event> Events { get; set; }

        public Tournament(API.Tournament API_Tournament)
        {
            Id = API_Tournament.Id.ToString();
            TournamentName = API_Tournament.Name;
            Events = new List<Event>();
            for (var i = 0; i < API_Tournament.Events.Count; i++)
            {
                Events.Add(new Event(API_Tournament.Events[i]));
            }
        }
    }
}
