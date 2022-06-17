namespace API_Scraper.Models
{
    public class Event : BaseDataObject
    {
        public string EventType { get; set; }
        public string EventName { get; set; }
        public List<Set> Sets { get; set; }

        public Event(API.Event API_Event)
        {
            EventName = API_Event.Name;
        }
    }
}
