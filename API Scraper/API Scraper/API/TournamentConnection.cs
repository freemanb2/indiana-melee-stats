using System.Collections.Generic;

namespace API_Scraper.API
{
    public class TournamentConnection
    {
        public PageInfo PageInfo { get; set; }
        public List<Tournament> Nodes { get; set; }
    }
}
