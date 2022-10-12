using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;

namespace API_Scraper.Models
{
    public class Set : BaseDataObject
    {
        public string DisplayScore { get; set; }
        public string WinnerId { get; set; }
        public string LoserId { get; set; }
        public List<Player> Players { get; set; }

        public Set(API.Set API_Set)
        {
            Id = API_Set.Id;
            DisplayScore = API_Set.DisplayScore;
            Players = new List<Player>();
            foreach(var slot in API_Set.Slots)
            {
                Players.Add(new Player(id: slot.Standing.Entrant.Participants[0].Player.Id.ToString(), gamerTag: slot.Standing.Entrant.Participants[0].Player.GamerTag));
            }
            WinnerId = API_Set.Slots.Where(slot => slot.Standing.Entrant.Id == API_Set.WinnerId).FirstOrDefault().Standing.Entrant.Participants[0].Player.Id.ToString();
            LoserId = WinnerId.ToString() == Players[0].Id ? Players[1].Id : Players[0].Id;
        }

        public Set(BsonDocument set)
        {
            Id = set.GetValue("_id").ToString();
            DisplayScore = set.GetValue("DisplayScore").ToString();
            WinnerId = set.GetValue("WinnerId").ToString();
            LoserId = set.GetValue("LoserId").ToString();
            Players = new List<Player>();

            var documentPlayers = set.GetValue("Players").AsBsonArray;
            foreach (var player in documentPlayers)
            {
                Players.Add(new Player(player.AsBsonDocument));
            }
        }
    }
}
