namespace API_Scraper.API
{
    public class Set
    {
        public int Id { get; set; }

        public string DisplayScore { get; set; }

        public int WinnerId { get; set; }

        public List<SetSlot> Slots { get; set; }

        public Event Event { get; set; }

    }
}
