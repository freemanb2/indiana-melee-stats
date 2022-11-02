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

            System.Console.WriteLine("Done");
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

            var client = new GraphQLHttpClient(config["GraphQLURI"], new NewtonsoftJsonSerializer());
            client.HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config["STARTGG_API_KEY"]);
            TournamentHandler _consumer = new TournamentHandler(client);

            validator = new DataValidator(_consumer, _db);
            writer = new DataWriter(_consumer, _db);
        }

        public async static Task ScrapeStartGGAPI(){
            int numLoops = 10;

            List<Tournament> validTournaments = new List<Tournament>();

            for (var i = 1; i <= numLoops; i++)
            {
                System.Console.WriteLine($"Fetching tournaments ({i} of {numLoops})");
                validTournaments = await validator.GetValidTournaments(numTournamentsToRecord: 10, numOnlineTournamentsToRecord: 10);
                ParseTournaments(validTournaments);
                if (i < numLoops)
                {
                    for (var j = 30; j >= 0; j--)
                    {
                        System.Console.Write($"\rWaiting {j} seconds before fetching more tournaments...");
                        System.Threading.Thread.Sleep(1000);
                    }
                    System.Console.WriteLine("");
                }
            }

            //Recalculate Elo ratings
            Console.WriteLine("Calculating updated Elo ratings...");
            try
            {
                //Elo_Calculator.Program.UpdateRatingsWithSpecificSets(setsToProcess); --Only works correctly when the newly ingested sets happened after all existing sets since sets have to be processed in chronological order
                Elo_Calculator.Program.Main();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
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
                                    var writtenSet = writer.WriteSet(set);
                                    setsToProcess.Add(writtenSet);
                                }
                                foreach (var player in set.Players)
                                {
                                    if (validator.IsValidPlayer(player))
                                    {
                                        writer.WritePlayer(player, tournament.Id);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
