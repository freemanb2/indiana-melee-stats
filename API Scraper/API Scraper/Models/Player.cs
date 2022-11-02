using MongoDB.Bson;
using System.Collections.Generic;

namespace API_Scraper.Models
{
    public class Player : BaseDataObject
    {
        public int Elo { get; set; }
        public string GamerTag { get; set; }
        public string Region { get; set; }
        public string MainCharacter { get; set; }
        public List<string> TournamentsAttended { get; set; }

        public Player(string id, string gamerTag, int elo = 1200, string region = "", string mainCharacter = "")
        {
            Id = id;
            Elo = elo;
            GamerTag = gamerTag;
            Region = region;
            MainCharacter = mainCharacter;
            TournamentsAttended = new List<string>();
        }

        public Player(BsonDocument player)
        {
            Id = player.GetValue("_id").ToString();
            Elo = player.GetValue("Elo").ToInt32();
            GamerTag = player.GetValue("GamerTag").ToString();
            Region = player.GetValue("Region").ToString();
            MainCharacter = player.GetValue("MainCharacter").ToString();

            var documentTournamentsAttended = player.GetValue("TournamentsAttended").AsBsonArray;
            foreach (var tournament in documentTournamentsAttended)
            {
                TournamentsAttended.Add(tournament.AsString);
            }
        }
    }
}
