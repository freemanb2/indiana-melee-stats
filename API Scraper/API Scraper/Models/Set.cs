using System.Linq;

namespace API_Scraper.Models
{
    public class Set : BaseDataObject
    {
        public string DisplayScore { get; set; }
        public int? WinnerId { get; set; }
        public int LoserId { get; set; }

        public Set(API.Set API_Set)
        {
            Id = API_Set.Id;
            DisplayScore = API_Set.DisplayScore;
            WinnerId = API_Set.WinnerId != null ? API_Set.WinnerId : 0;
        }
    }
}
