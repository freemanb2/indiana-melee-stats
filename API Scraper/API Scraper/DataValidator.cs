using System.Collections.Generic;

using MongoDB.Driver;
using API_Scraper.Models;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace API_Scraper
{
    public class DataValidator
    {
        public async Task<List<Tournament>> GetValidTournaments(IMongoDatabase _db, SetConsumer _consumer, int numTournamentsToRecord)
        {
            var _tournaments = _db.GetCollection<BsonDocument>("Tournaments");
            DataWriter writer = new DataWriter(_db);

            List<string> recentTournamentIds = await _consumer.GetRecentIndianaTournamentIds(500);

            var numTournamentsRecorded = 0;
            List<Tournament> validTournaments = new List<Tournament>();

            foreach (var tournamentId in recentTournamentIds)
            {
                if (!DocumentExists(_tournaments, tournamentId))
                {
                    API.Tournament results = await _consumer.GetSpecificTournamentResults(tournamentId: tournamentId);
                    Tournament tournament = new Tournament(results);
                    if (IsValidTournament(tournament))
                    {
                        validTournaments.Add(tournament);
                        numTournamentsRecorded++;
                    }
                    else
                    {
                        writer.WriteTournament(tournament, false);
                    }
                }

                if (numTournamentsRecorded == numTournamentsToRecord) break;
            }

            return validTournaments;
        }

        public bool DocumentExists(IMongoCollection<BsonDocument> collection, string id)
        {
            return collection.Find(new BsonDocument { { "_id", id } }).CountDocuments() > 0;
        }

        #region document validation
        public bool IsValidTournament(Tournament tournament)
        {
            foreach (Event _event in tournament.Events)
            {
                if (IsValidEvent(_event))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsValidEvent(Event _event)
        {
            if (_event.EventName.ToLower().Contains("amateur")) return false;
            if (!_event.EventName.ToLower().Contains("singles")) return false;
            if (!_event.State.ToLower().Equals("completed") && !_event.State.ToLower().Equals("active")) return false;
            foreach (Set set in _event.Sets)
            {
                if (IsValidSet(set))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsValidSet(Set set)
        {
            return set.WinnerId != null && set.DisplayScore != "DQ";
        }

        public bool IsValidPlayer(Player player)
        {
            //Criteria for valid player? Do invalid players exist?
            return true;
        }
        #endregion
    }
}
