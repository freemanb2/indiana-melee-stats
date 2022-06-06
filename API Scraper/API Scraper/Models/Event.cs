namespace API_Scraper
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Tournament tournament { get; set; }
    }
}
