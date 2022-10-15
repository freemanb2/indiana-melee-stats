using System.Collections.Generic;

using MongoDB.Driver;
using API_Scraper.Models;
using MongoDB.Bson;

namespace API_Scraper
{
    public class DataWriter
    {
        private readonly IMongoCollection<BsonDocument> _tournaments;
        private readonly IMongoCollection<BsonDocument> _events;
        private readonly IMongoCollection<BsonDocument> _sets;
        private readonly IMongoCollection<BsonDocument> _players;
        private readonly DataValidator _validator;

        public DataWriter(IMongoDatabase db)
        {
            _tournaments = db.GetCollection<BsonDocument>("Tournaments");
            _events = db.GetCollection<BsonDocument>("Events");
            _sets = db.GetCollection<BsonDocument>("Sets");
            _players = db.GetCollection<BsonDocument>("Players");
            _validator = new DataValidator();
        }

        #region public methods
        public void WriteTournament(Tournament _tournament) {
            if (_validator.DocumentExists(_tournaments, _tournament.Id)) return;

            var tournamentDocument = CreateTournamentDocument(_tournament);
            try
            {
                _tournaments.InsertOne(tournamentDocument);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Exception while writing Tournament with id {_tournament.Id} and name {_tournament.TournamentName}: ", e.Message);
            }
        }

        public void WriteInvalidTournament(Tournament _tournament)
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
        }

        public void WriteEvent(Event _event)
        {
            if (_validator.DocumentExists(_events, _event.Id)) return;

            var eventDocument = CreateEventDocument(_event);
            try
            {
                _events.InsertOne(eventDocument);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Exception while writing Event with id {_event.Id} and name {_event.EventName}: ", e.Message);
            }
        }

        public void WriteSet(Set _set)
        {
            if (_validator.DocumentExists(_sets, _set.Id)) return;

            var setDocument = CreateSetDocument(_set);
            try
            {
                _sets.InsertOne(setDocument);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Exception while writing Set with id {_set.Id}: ", e.Message);
            }
        }

        public void WritePlayer(Player _player)
        {
            if (_validator.DocumentExists(_players, _player.Id)) return;

            var playerDocument = CreatePlayerDocument(_player);
            if(!_validator.DocumentExists(_players, playerDocument["_id"].ToString())){
                try
                {
                    _players.InsertOne(playerDocument);
                }
                catch (System.Exception e)
                {
                    System.Console.WriteLine($"Exception while writing Player with id {playerDocument["_id"].ToString()} and gamerTag {playerDocument["GamerTag"].ToString()}: ", e.Message);
                }
            }
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
            return new BsonDocument {
                {"_id", set.Id },
                {"DisplayScore", set.DisplayScore },
                {"WinnerId", set.WinnerId },
                {"LoserId", set.LoserId },
                {"TotalGames", set.TotalGames },
                {"PlayerIds", GetPlayerIdsForSet(set.Players) },
                {"Players", CreateSetPlayers(set.Players) },
                {"Stale", set.Stale },
                {"CompletedAt", set.CompletedAt }
            };
        }

        private BsonArray GetPlayerIdsForSet(List<Player> players)
        {
            var playerIdsBsonArray = new BsonArray();
            foreach (Player player in players)
            {
                playerIdsBsonArray.Add(player.Id);
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
            BsonDocument existingPlayer = _players.Find(x => x["GamerTag"] == player.GamerTag).SingleOrDefault();
            if (existingPlayer != null)
            {
                return existingPlayer.ToBsonDocument();
            }

            return new BsonDocument {
                {"_id", player.Id },
                {"Elo", player.Elo },
                {"GamerTag", player.GamerTag },
                {"Region", player.Region },
                {"MainCharacter", player.MainCharacter }
            };
        }
        #endregion
    }
}
