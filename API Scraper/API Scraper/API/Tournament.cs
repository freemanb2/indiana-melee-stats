using System.Collections.Generic;
using System;

namespace API_Scraper.API
{
    public class Tournament
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public int StartAt { get; set; }
        public List<Event> Events { get; set; }
    }
}
