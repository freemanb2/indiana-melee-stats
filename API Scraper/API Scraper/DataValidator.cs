using System.Collections.Generic;

using MongoDB.Driver;
using API_Scraper.Models;
using MongoDB.Bson;
using System.Threading.Tasks;
using System;

namespace API_Scraper
{
    public class DataValidator
    {
        public async Task<List<Tournament>> GetValidTournaments(IMongoDatabase _db, TournamentHandler _consumer, int numTournamentsToRecord)
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
                }

                if (numTournamentsRecorded == numTournamentsToRecord) break;
            }

            return validTournaments;
        }

        public List<Tournament> GetRecentIncompleteTournaments(IMongoDatabase _db)
        {
            var _tournaments = _db.GetCollection<BsonDocument>("Tournaments");
            var recentIncompleteTournamentDocuments = _tournaments.Find(x => x["Completed"] == false && x["Date"] >= DateTime.Now.AddDays(-30)).ToList();
            var recentIncompleteTournaments = new List<Tournament>();

            foreach(var document in recentIncompleteTournamentDocuments)
            {
                recentIncompleteTournaments.Add(new Tournament(document));
            }

            return recentIncompleteTournaments;
        }

        public bool DocumentExists(IMongoCollection<BsonDocument> collection, string id)
        {
            return collection.Find(x => x["_id"] == id).CountDocuments() > 0;
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

        public bool IsCompletedTournament(Tournament tournament)
        {
            foreach (var e in tournament.Events)
            {
                if (IsValidEvent(e) && !IsCompletedEvent(e))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsValidEvent(Event _event)
        {
            if (_event.EventName.ToLower().Contains("amateur")) return false;
            if (!_event.EventName.ToLower().Contains("singles")) return false;
            if (!_event.State.ToLower().Equals("completed") && !_event.State.ToLower().Equals("active")) return false;
            return true;
        }

        public bool IsCompletedEvent(Event _event)
        {
            return _event.State.ToLower().Equals("completed");
        }

        public bool IsValidSet(Set set)
        {
            //Set is finished and was not a DQ
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
