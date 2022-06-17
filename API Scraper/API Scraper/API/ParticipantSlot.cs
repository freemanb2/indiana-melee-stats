using Newtonsoft.Json;

namespace API_Scraper.API
{
    public class ParticipantSlot
    {
        public string Id { get; set; }

        public Participant Participant { get; set; }
    }
}