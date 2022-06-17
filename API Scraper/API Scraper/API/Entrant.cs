namespace API_Scraper.API
{
    public class Entrant
    {
        public int Id { get; set; }
        public Event Event { get; set; }
        public int InitialSeedNum { get; set; }
        public bool IsDisqualified { get; set; }
        public string Name { get; set; }
        public SetConnection PaginatedSets { get; set; }
        public List<Participant> Participants { get; set; }
        public List<int> Seeds { get; set; }
        public int Skill { get; set; }
        public Standing Standing { get; set; }
    }
}