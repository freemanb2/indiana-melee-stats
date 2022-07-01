using System.Collections.Generic;

namespace API_Scraper.Models
{
    public class Event : BaseDataObject
    {
        public string EventType { get; set; }
        public string EventName { get; set; }
        public string State { get; set; }
        public List<Set> Sets { get; set; }

        public Event(API.Event API_Event)
        {
            Id = API_Event.Id.ToString();
            EventType = API_Event.Type == 1 ? "Singles" : "Doubles";
            EventName = API_Event.Name;
            State = API_Event.State;
            Sets = new List<Set>();

            for (var i = 0; i < API_Event.Sets.Nodes.Count; i++)
            {
                Sets.Add(new Set(API_Event.Sets.Nodes[i]));
            }
        }
    }
}
