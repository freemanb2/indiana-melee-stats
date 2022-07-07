using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using System.Threading.Tasks;
using System.Collections.Generic;

using MongoDB.Driver;

using API_Scraper.Models;
using MongoDB.Bson;

namespace API_Scraper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            var task = ScrapeStartGGAPI();
            task.Wait();
        }

        public async static Task ScrapeStartGGAPI(){
            var client = new GraphQLHttpClient("https://api.start.gg/gql/alpha", new NewtonsoftJsonSerializer());
            client.HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "fa9a0aa93bf21b04a207eb364549e31b");
            SetConsumer _consumer = new SetConsumer(client);

            MongoClient dbClient = new MongoClient("mongodb://localhost:27017/SmashStatsDB");
            var _db = dbClient.GetDatabase("SmashStatsDB");
            var _tournaments = _db.GetCollection<BsonDocument>("Tournaments");
            var _events = _db.GetCollection<BsonDocument>("Events");
            var _sets = _db.GetCollection<BsonDocument>("Sets");
            var _players = _db.GetCollection<BsonDocument>("Players");

            Tournament tournament;
            List<API.Tournament> results;

            var numTournamentsToRecord = 10;
            var recentTournamentIds = await _consumer.GetRecentIndianaTournamentIds();
            var tournamentsToQuery = GetMostRecentUnrecordedTournaments(_tournaments, recentTournamentIds, numTournamentsToRecord);

            foreach (var tournamentId in tournamentsToQuery)
            {
                results = await _consumer.GetSpecificTournamentResults(tournamentId: tournamentId);
                foreach (API.Tournament result in results)
                {
                    tournament = new Tournament(result);
                    if(IsValidTournament(tournament)){
                        System.Console.WriteLine("Recording Tournament: " + tournament.TournamentName);
                        if (!DocumentExists(_tournaments, tournament.Id))
                        {
                            var tournamentDocument = CreateTournamentDocument(tournament);
                            _tournaments.InsertOne(tournamentDocument);
                        }
                        foreach (Event _event in tournament.Events)
                        {
                            if (IsValidEvent(_event))
                            {
                                if (!DocumentExists(_events, _event.Id))
                                {
                                    var eventDocument = CreateEventDocument(_event);
                                    _events.InsertOne(eventDocument);
                                }
                                foreach (Set set in _event.Sets)
                                {
                                    if (IsValidSet(set))
                                    {
                                        if (!DocumentExists(_sets, set.Id))
                                        {
                                            var setDocument = CreateSetDocument(set);
                                            _sets.InsertOne(setDocument);
                                        }
                                        foreach (var player in set.Players)
                                        {
                                            if (!DocumentExists(_players, player.Id))
                                            {
                                                var playerDocument = CreatePlayerDocument(player);
                                                _players.InsertOne(playerDocument);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            System.Console.WriteLine("Done");
            
        }

        public static List<string> GetMostRecentUnrecordedTournaments(IMongoCollection<BsonDocument> _tournaments, List<string> recentTournamentIds, int numTournamentsToRecord)
        {
            List<string> mostRecentUnrecordedTournaments = new List<string>();
            int index = 0;

            while(mostRecentUnrecordedTournaments.Count < numTournamentsToRecord && index < recentTournamentIds.Count)
            {
                if (!DocumentExists(_tournaments, recentTournamentIds[index]))
                {
                    mostRecentUnrecordedTournaments.Add(recentTournamentIds[index]);
                }
                index++;
            }

            return mostRecentUnrecordedTournaments;
        }

        public static bool DocumentExists(IMongoCollection<BsonDocument> collection, string id)
        {
            return collection.Find(new BsonDocument { { "_id", id } }).CountDocuments() > 0;
        }

        public static bool IsValidTournament(Tournament tournament){
            bool hasValidEvent = false;
            foreach (Event _event in tournament.Events) {
                if (IsValidEvent(_event)) { 
                    hasValidEvent = true;
                }
            }
            return hasValidEvent;
        }

        public static BsonDocument CreateTournamentDocument(Tournament tournament)
        {
            return new BsonDocument {
                {"_id", tournament.Id },
                {"TournamentName", tournament.TournamentName },
                {"Date", tournament.Date },
                {"Events", CreateTournamentEvents(tournament.Events) }
            };
        }

        public static BsonArray CreateTournamentEvents(List<Event> events)
        {
            var eventsBsonArray = new BsonArray();
            foreach(Event e in events)
            {
                eventsBsonArray.Add(CreateEventDocument(e));
            }
            return eventsBsonArray;
        }

        public static bool IsValidEvent(Event _event)
        {
            if(_event.EventName.ToLower().Contains("amateur")) return false;

            var valid = false;
            foreach(Set set in _event.Sets){
                if(IsValidSet(set)){
                    valid = true;
                }
            }
            return valid;
        }

        public static BsonDocument CreateEventDocument(Event _event)
        {
            return new BsonDocument {
                {"_id", _event.Id },
                {"EventName", _event.EventName },
                {"EventType", _event.EventType },
                {"Sets", CreateEventSets(_event.Sets) }
            };
        }

        public static bool IsValidSet(Set set)
        {
            return set.WinnerId != null && set.DisplayScore != "DQ";
        }

        public static BsonArray CreateEventSets(List<Set> sets)
        {
            var setsBsonArray = new BsonArray();
            foreach (Set set in sets)
            {
                setsBsonArray.Add(CreateSetDocument(set));
            }
            return setsBsonArray;
        }

        public static BsonDocument CreateSetDocument(Set set)
        {
            return new BsonDocument {
                {"_id", set.Id },
                {"DisplayScore", set.DisplayScore },
                {"WinnerId", set.WinnerId },
                {"LoserId", set.LoserId },
                {"PlayerIds", GetPlayerIdsForSet(set.Players) },
                {"Players", CreateSetPlayers(set.Players) }
            };
        }

        public static BsonArray GetPlayerIdsForSet(List<Player> players){
            var playerIdsBsonArray = new BsonArray();
            foreach (Player player in players){
                playerIdsBsonArray.Add(player.Id);
            }
            return playerIdsBsonArray;
        }

        public static BsonArray CreateSetPlayers(List<Player> players)
        {
            var playersBsonArray = new BsonArray();
            foreach (Player player in players)
            {
                playersBsonArray.Add(CreatePlayerDocument(player));
            }
            return playersBsonArray;
        }

        public static BsonDocument CreatePlayerDocument(Player player)
        {
            return new BsonDocument {
                {"_id", player.Id },
                {"Elo", player.Elo },
                {"GamerTag", player.GamerTag },
                {"Region", player.Region },
                {"MainCharacter", player.MainCharacter }
            };
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
