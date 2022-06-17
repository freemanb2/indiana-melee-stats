namespace API_Scraper.API
{
    public class SetSlot
    {
        public string Id { get; set; }
        public Entrant Entrant { get; set; }
        public int Seed { get; set; }
        public int SlotIndex { get; set; }
        public Standing Standing { get; set; }
    }
}