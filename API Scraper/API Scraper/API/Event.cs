namespace API_Scraper.API
{
    public class Event
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public Tournament Tournament { get; set; }
        public EntrantConnection Entrants { get; set; }
        public SetConnection Sets { get; set; }
    }
}
