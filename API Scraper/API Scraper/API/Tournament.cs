using System.Collections.Generic;

namespace API_Scraper.API
{
    public class Tournament
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Event> Events { get; set; }
    }
}
