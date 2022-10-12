using System.Collections.Generic;
using System;
using MongoDB.Bson;

namespace API_Scraper.Models
{
    public class Tournament : BaseDataObject
    {
        public string TournamentName { get; set; }
        public string Link { get; set; }
        public DateTime Date { get; set; }
        public List<Event> Events { get; set; }

        public Tournament(API.Tournament API_Tournament)
        {
            Id = API_Tournament.Id.ToString();
            TournamentName = API_Tournament.Name;
            Link = "https://www.start.gg/" + API_Tournament.Slug;
            Date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(API_Tournament.StartAt);
            Events = new List<Event>();
            for (var i = 0; i < API_Tournament.Events.Count; i++)
            {
                Events.Add(new Event(API_Tournament.Events[i]));
            }
        }

        public Tournament(BsonDocument tournament)
        {
            Id = tournament.GetValue("_id").ToString();
            TournamentName = tournament.GetValue("TournamentName").ToString();
            Link = tournament.GetValue("Link").ToString();
            Date = tournament.GetValue("Date").ToUniversalTime();
            Events = new List<Event>();

            var documentEvents = tournament.GetValue("Events").AsBsonArray;
            foreach (var document in documentEvents)
            {
                Events.Add(new Event(document.AsBsonDocument));
            }
        }
    }
}
