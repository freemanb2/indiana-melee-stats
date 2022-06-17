namespace API_Scraper.API
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Tournament Tournament { get; set; }
        public EntrantConnection Entrants { get; set; }
    }
}
