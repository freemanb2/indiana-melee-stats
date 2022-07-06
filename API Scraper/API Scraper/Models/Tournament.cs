using System.Collections.Generic;
using System;

namespace API_Scraper.Models
{
    public class Tournament : BaseDataObject
    {
        public string TournamentName { get; set; }
        public DateTime Date { get; set; }
        public List<Event> Events { get; set; }

        public Tournament(API.Tournament API_Tournament)
        {
            Id = API_Tournament.Id.ToString();
            TournamentName = API_Tournament.Name;
            Date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(API_Tournament.StartAt);
            Events = new List<Event>();
            for (var i = 0; i < API_Tournament.Events.Count; i++)
            {
                Events.Add(new Event(API_Tournament.Events[i]));
            }
        }
    }
}
