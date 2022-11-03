using System.Collections.Generic;

using MongoDB.Driver;
using API_Scraper.Models;
using MongoDB.Bson;
using System;
using System.Text.RegularExpressions;

namespace API_Scraper
{
    public class DataWriter
    {
        private readonly IMongoCollection<BsonDocument> _tournaments;
        private readonly IMongoCollection<BsonDocument> _events;
        private readonly IMongoCollection<BsonDocument> _sets;
        private readonly IMongoCollection<BsonDocument> _players;
        private readonly DataValidator _validator;

        public DataWriter(TournamentHandler consumer, IMongoDatabase db)
        {
            _tournaments = db.GetCollection<BsonDocument>("Tournaments");
            _events = db.GetCollection<BsonDocument>("Events");
            _sets = db.GetCollection<BsonDocument>("Sets");
            _players = db.GetCollection<BsonDocument>("Players");
            _validator = new DataValidator(consumer, db);
        }

        #region public methods
        public BsonDocument WriteTournament(Tournament _tournament) {
            if (_validator.DocumentExists(_tournaments, _tournament.Id)) return null;

            var tournamentDocument = CreateTournamentDocument(_tournament);
            try
            {
                _tournaments.InsertOne(tournamentDocument);
                return tournamentDocument;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Exception while writing Tournament with id {_tournament.Id} and name {_tournament.TournamentName}: ", e.Message);
                return null;
            }
        }

        public BsonDocument WriteInvalidTournament(Tournament _tournament)
        {
            var tournamentDocument = new BsonDocument
            {
                {"_id", _tournament.Id},
                {"TournamentName", _tournament.TournamentName},
                {"Link", _tournament.Link },
                {"Date", _tournament.Date },
                {"Valid", false }
            };

            _tournaments.InsertOne(tournamentDocument);
            return tournamentDocument;
        }

        public BsonDocument WriteEvent(Event _event)
        {
            if (_validator.DocumentExists(_events, _event.Id)) return null;

            var eventDocument = CreateEventDocument(_event);
            try
            {
                _events.InsertOne(eventDocument);
                return eventDocument;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Exception while writing Event with id {_event.Id} and name {_event.EventName}: ", e.Message);
                return null;
            }
        }

        public BsonDocument WriteSet(Set _set)
        {
            if (_validator.DocumentExists(_sets, _set.Id)) return null;

            var setDocument = CreateSetDocument(_set);
            try
            {
                _sets.InsertOne(setDocument);
                return setDocument;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Exception while writing Set with id {_set.Id}: ", e.Message);
                return null;
            }
        }

        public BsonDocument WritePlayer(Player _player, string tournamentId = null)
        {
            if (_validator.DocumentExists(_players, _player.Id))
            {
                UpdateGamerTag(_player);
                InsertTournamentAttended(_player, tournamentId);
                return null;
            }

            var playerDocument = CreatePlayerDocument(_player);
            if(!_validator.DocumentExists(_players, playerDocument["_id"].ToString())){
                try
                {
                    _players.InsertOne(playerDocument);
                }
                catch (System.Exception e)
                {
                    System.Console.WriteLine($"Exception while writing Player with id {playerDocument["_id"].ToString()} and gamerTag {playerDocument["GamerTag"].ToString()}: ", e.Message);
                    return null;
                }
            }

            InsertTournamentAttended(new Player(playerDocument), tournamentId);
            return playerDocument;
        }
        #endregion

        #region helper methods
        private BsonDocument CreateTournamentDocument(Tournament tournament)
        {
            return new BsonDocument {
                {"_id", tournament.Id },
                {"TournamentName", tournament.TournamentName },
                {"Link", tournament.Link },
                {"Date", tournament.Date },
                {"Events", CreateTournamentEvents(tournament.Events) },
                {"Completed", _validator.IsCompletedTournament(tournament) }
            };
        }

        private BsonArray CreateTournamentEvents(List<Event> events)
        {
            var eventsBsonArray = new BsonArray();
            foreach (Event e in events)
            {
                eventsBsonArray.Add(CreateEventDocument(e));
            }
            return eventsBsonArray;
        }

        private BsonDocument CreateEventDocument(Event _event)
        {
            return new BsonDocument {
                {"_id", _event.Id },
                {"EventName", _event.EventName },
                {"EventType", _event.EventType },
                {"State", _event.State },
                {"NumEntrants", _event.NumEntrants },
                {"Sets", CreateEventSets(_event.Sets) }
            };
        }

        private BsonArray CreateEventSets(List<Set> sets)
        {
            var setsBsonArray = new BsonArray();
            foreach (Set set in sets)
            {
                setsBsonArray.Add(CreateSetDocument(set));
            }
            return setsBsonArray;
        }

        private BsonDocument CreateSetDocument(Set set)
        {
            // Deduplicate Player data before inserting into Set document
            BsonArray setPlayers = CreateSetPlayers(set.Players);

            return new BsonDocument {
                {"_id", set.Id },
                {"DisplayScore", set.DisplayScore },
                {"WinnerId", GetWinnerIdForSet(set, setPlayers) },
                {"LoserId", GetLoserIdForSet(set, setPlayers) },
                {"TotalGames", set.TotalGames },
                {"PlayerIds", GetPlayerIdsForSet(setPlayers) },
                {"Players", setPlayers },
                {"Stale", set.Stale },
                {"CompletedAt", set.CompletedAt }
            };
        }

        private string GetWinnerIdForSet(Set set, BsonArray setPlayers)
        {
            var winnerTag = set.Players.Find(x => x.Id == set.WinnerId).GamerTag;
            var gamerTagRegex = new Regex("^"+Regex.Escape(winnerTag)+"$", RegexOptions.IgnoreCase);
            var winner = setPlayers.ToList().Find(x => gamerTagRegex.IsMatch(x["GamerTag"].AsString)).AsBsonDocument;
            return winner["_id"].AsString;
        }

        private string GetLoserIdForSet(Set set, BsonArray setPlayers)
        {
            var loserTag = set.Players.Find(x => x.Id == set.LoserId).GamerTag;
            var gamerTagRegex = new Regex("^" + Regex.Escape(loserTag) + "$", RegexOptions.IgnoreCase);
            var loser = setPlayers.ToList().Find(x => gamerTagRegex.IsMatch(x["GamerTag"].AsString)).AsBsonDocument;
            return loser["_id"].AsString;
        }

        private BsonArray GetPlayerIdsForSet(BsonArray players)
        {
            var playerIdsBsonArray = new BsonArray();
            foreach (BsonDocument player in players)
            {
                playerIdsBsonArray.Add(player["_id"]);
            }
            return playerIdsBsonArray;
        }

        private BsonArray CreateSetPlayers(List<Player> players)
        {
            var playersBsonArray = new BsonArray();
            foreach (Player player in players)
            {
                playersBsonArray.Add(CreatePlayerDocument(player));
            }
            return playersBsonArray;
        }

        private BsonDocument CreatePlayerDocument(Player player)
        {
            var filter = Builders<BsonDocument>.Filter.Regex("GamerTag", new BsonRegularExpression("^"+Regex.Escape(player.GamerTag)+"$", "i"));
            BsonDocument existingPlayer = new BsonDocument();
            try
            {
                existingPlayer = _players.Find(filter).SingleOrDefault();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (existingPlayer != null)
            {
                if (existingPlayer["GamerTag"].AsString != player.GamerTag)
                {
                    UpdateGamerTag(player, existingPlayer);
                }
                return existingPlayer.ToBsonDocument();
            }

            return new BsonDocument {
                {"_id", player.Id },
                {"Elo", player.Elo },
                {"GamerTag", player.GamerTag },
                {"Region", player.Region },
                {"MainCharacter", player.MainCharacter },
                {"TournamentsAttended", new BsonArray() }
            };
        }

        private void UpdateGamerTag(Player ingestedPlayer, BsonDocument existingPlayer = null)
        {
            if (existingPlayer == null)
            {
                var playerFilter = Builders<BsonDocument>.Filter.Eq("_id", ingestedPlayer.Id);
                existingPlayer = _players.Find(playerFilter).Single().AsBsonDocument;
                if (existingPlayer.GetValue("GamerTag").AsString == ingestedPlayer.GamerTag) return;
            }

            var setFilter = Builders<BsonDocument>.Filter.Eq("Players._id", existingPlayer.GetValue("_id"));
            var setsPlayed = _sets.Find(setFilter).ToList();

            setsPlayed.Sort((x, y) => x["CompletedAt"].CompareTo(y["CompletedAt"]));

            var mostRecentSet = setsPlayed[0];
            var setPlayers = mostRecentSet["Players"];
            var mostRecentGamerTag = setPlayers.AsBsonArray.ToList().Find(x => x.AsBsonDocument.GetValue("_id") == existingPlayer.GetValue("_id")).AsBsonDocument.GetValue("GamerTag").AsString;
            if (mostRecentGamerTag != existingPlayer.GetValue("GamerTag").AsString)
            {
                var updatePlayerFilter = Builders<BsonDocument>.Update.Set("GamerTag", mostRecentGamerTag);
                _players.UpdateOne(x => x.GetValue("_id") == existingPlayer.GetValue("_id"), updatePlayerFilter);
            }
        }

        private void InsertTournamentAttended(Player player, string? tournamentId)
        {
            if (tournamentId == null) return;

            var playerFilter = Builders<BsonDocument>.Filter.Eq("_id", player.Id);
            var playerDocument = _players.Find(playerFilter).Single().AsBsonDocument;

            List<BsonValue> tournamentsAttended = playerDocument.GetValue("TournamentsAttended").AsBsonArray.ToList();
            if (tournamentsAttended.Contains(tournamentId)) return;

            tournamentsAttended.Add(tournamentId);
            var update = Builders<BsonDocument>.Update.Set("TournamentsAttended", tournamentsAttended);
            try
            {
                _players.UpdateOne(x => x["_id"] == player.Id, update);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion
    }
}
