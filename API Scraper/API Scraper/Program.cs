using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using System.Threading.Tasks;
using System.Collections.Generic;

using MongoDB.Driver;
using API_Scraper.Models;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using Elo_Calculator;
using MongoDB.Bson;

namespace API_Scraper
{
    public class Program
    {
        private static DataValidator validator { get; set; }
        private static DataWriter writer { get; set; }
        private static IConfigurationRoot config { get; set; }
        private static IMongoDatabase _db { get; set; } 


        public static void Main(string[] args)
        {
            InitializeDatabase();

            //Elo_Calculator.Program.MarkStaleSets();
            //ReprocessRecentIncompleteTournaments();

            var task = ScrapeStartGGAPI();
            task.Wait();
        }

        private static void InitializeDatabase()
        {
            var cd = Environment.CurrentDirectory;
            var projectDirectory = Directory.GetParent(cd).Parent.Parent.FullName;
            var dotenv = Path.Combine(projectDirectory, ".env");
            DotEnv.Load(dotenv);

            config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            MongoClient dbClient = new MongoClient(config["MONGODB_PATH"]);
            _db = dbClient.GetDatabase("IndianaMeleeStatsDB");

            validator = new DataValidator();
            writer = new DataWriter(_db);
        }

        public async static Task ScrapeStartGGAPI(){
            var client = new GraphQLHttpClient(config["GraphQLURI"], new NewtonsoftJsonSerializer());
            client.HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config["STARTGG_API_KEY"]);
            TournamentHandler _consumer = new TournamentHandler(client);
            List<Tournament> validTournaments = await validator.GetValidTournaments(_db, _consumer, numTournamentsToRecord: 40);

            ParseTournaments(validTournaments);
        }

        // Look for incomplete tournaments that happened in the last 30 days and query for updates that may have occurred.
        private static void ReprocessRecentIncompleteTournaments()
        {
            Console.WriteLine("Reprocessing incomplete tournaments from the last 7 days");
            var recentIncompleteTournaments = validator.GetRecentIncompleteTournaments(_db);
            ParseTournaments(recentIncompleteTournaments);
        }

        private static void ParseTournaments(List<Tournament> tournaments)
        {
            List<BsonDocument> setsToProcess = new List<BsonDocument>();
            var _sets = _db.GetCollection<BsonDocument>("Sets");

            foreach (var tournament in tournaments)
            {
                Console.WriteLine("Recording Tournament: " + tournament.TournamentName);
                writer.WriteTournament(tournament);
                foreach (Event _event in tournament.Events)
                {
                    if (validator.IsValidEvent(_event))
                    {
                        writer.WriteEvent(_event);
                        foreach (Set set in _event.Sets)
                        {
                            if (validator.IsValidSet(set))
                            {
                                if (!validator.DocumentExists(_sets, set.Id))
                                {
                                    setsToProcess.Add(set.ToBsonDocument());
                                }
                                writer.WriteSet(set);
                                foreach (var player in set.Players)
                                {
                                    if (validator.IsValidPlayer(player))
                                    {
                                        writer.WritePlayer(player);
                                    }
                                }
                            }
                        }
                    }
                }
                // Recalculate Elo ratings if tournament happened in the last 180 days
                //if (tournament.Date >= DateTime.Now.AddYears(-1) && setsToProcess.Count > 0)
                //{
                //    Elo_Calculator.Program.UpdateRatingsWithSpecificSets(setsToProcess);
                //}
                setsToProcess.Clear();
            }
            System.Console.WriteLine("Done");
        }
    }
}
