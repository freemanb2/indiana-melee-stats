using System.Collections.Generic;

namespace API_Scraper.API
{
    public class Set
    {
        public string Id { get; set; }

        public string DisplayScore { get; set; }

        public int? WinnerId { get; set; }
        public int TotalGames { get; set; }

        public List<SetSlot> Slots { get; set; }

        public Event Event { get; set; }

        public int CompletedAt { get; set; }
    }
}
