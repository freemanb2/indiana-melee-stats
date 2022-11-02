using System.Collections.Generic;

using MongoDB.Driver;
using API_Scraper.Models;
using MongoDB.Bson;
using System.Threading.Tasks;
using System;
using System.Text.RegularExpressions;

namespace API_Scraper
{
    public class DataValidator
    {
        private TournamentHandler _consumer;
        private IMongoDatabase _db;

        public DataValidator(TournamentHandler _consumer, IMongoDatabase _db)
        {
            this._consumer = _consumer;
            this._db = _db;
        }

        public async Task<List<Tournament>> GetValidTournaments(int numTournamentsToRecord, int numOnlineTournamentsToRecord)
        {
            var _tournaments = _db.GetCollection<BsonDocument>("Tournaments");
            var writer = new DataWriter(_consumer, _db);

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
                        writer.WriteInvalidTournament(tournament);
                    }
                }

                if (numTournamentsRecorded == numTournamentsToRecord) break;
            }

            var validOnlineTournaments = await GetValidOnlineTournaments();
            recentTournamentIds = await _consumer.GetRecentIndianaOnlineTournamentIds(validOnlineTournaments);
            numTournamentsRecorded = 0;

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
                        writer.WriteInvalidTournament(tournament);
                    }
                }

                if (numTournamentsRecorded == numOnlineTournamentsToRecord) break;
            }

            return validTournaments;
        }

        public List<Tournament> GetRecentIncompleteTournaments(IMongoDatabase _db)
        {
            var _tournaments = _db.GetCollection<BsonDocument>("Tournaments");
            var recentIncompleteTournamentDocuments = _tournaments.Find(x => x["Completed"] == false && x["Date"] >= DateTime.Now.AddDays(-7)).ToList();
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

        private async Task<List<Tuple<string,string>>> GetValidOnlineTournaments()
        {
            var validOnlineTournaments = new List<Tuple<string, string>>
            {
                new Tuple<string,string>("282411", "State of Affairs"), //Acid
                new Tuple<string,string>("370809", "Crossroads"), //dalbull
                new Tuple<string,string>("429020", "Crossroads") //Blue
            };

            return validOnlineTournaments;
        }

        private async Task<string> GetUserIdOfPlayer(string gamerTag)
        {
            var _players = _db.GetCollection<BsonDocument>("Players");

            var filter = Builders<BsonDocument>.Filter.Regex("GamerTag", new BsonRegularExpression("^" + Regex.Escape(gamerTag) + "$", "i"));
            var playerId = _players.Find(filter).Single().GetValue("_id").AsString;
            var userId = await _consumer.GetUserIdOfPlayer(playerId);

            return userId;
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
