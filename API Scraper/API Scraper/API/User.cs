using System.Collections.Generic;

namespace API_Scraper.API
{
    public class User
    {
        public int Id { get; set; }
        public string GenderPronoun { get; set; }
        public Address Location { get; set; }
        public string Name { get; set; }
        public Player Player { get; set; }
        public string Slug { get; set; }
        public TournamentConnection Tournaments { get; set; }
    }
}
