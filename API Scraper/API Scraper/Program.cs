using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using System.Threading.Tasks;
using System.Collections.Generic;

using MongoDB.Driver;
using API_Scraper.Models;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace API_Scraper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, ".env");
            DotEnv.Load(dotenv);

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            ReprocessRecentIncompleteTournaments(config);

            var task = ScrapeStartGGAPI(config);
            task.Wait();
        }

        public async static Task ScrapeStartGGAPI(IConfigurationRoot config){
            var client = new GraphQLHttpClient(config["GraphQLURI"], new NewtonsoftJsonSerializer());
            client.HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config["STARTGG_API_KEY"]);
            SetConsumer _consumer = new SetConsumer(client);
            MongoClient dbClient = new MongoClient(config["MONGODB_PATH"]);
            var _db = dbClient.GetDatabase("IndianaMeleeStatsDB");

            DataValidator validator = new DataValidator();
            DataWriter writer = new DataWriter(_db);
            List<Tournament> validTournaments = await validator.GetValidTournaments(_db, _consumer, numTournamentsToRecord: 20);

            ParseTournaments(validTournaments, writer, validator);
        }

        // Look for incomplete tournaments that happened in the last 30 days and query for updates that may have occurred.
        // Context: A tournament could be processed intially while it's still happening, or if the TO hasn't finalized the brackets yet. 
        //          Tournaments are also picked up in the "Created" state if the query happens on the day they're scheduled to happen. These tournaments are marked as incomplete.
        //          We want to continuously query for changes to these tournaments over the next 30 days to check and see if the bracket was finalized as some point so we can make sure that we've accounted for all sets that occurred.
        //          After 30 days, we can safely assume that the tournament never actually happened, or the TO is never going to finalize the bracket properly, so stop checking for updates.
        private static void ReprocessRecentIncompleteTournaments(IConfigurationRoot config)
        {
            System.Console.WriteLine("Reprocessing incomplete tournaments from the last 30 days");
            MongoClient dbClient = new MongoClient(config["MONGODB_PATH"]);
            var _db = dbClient.GetDatabase("IndianaMeleeStatsDB");
            DataValidator validator = new DataValidator();
            DataWriter writer = new DataWriter(_db);

            var recentIncompleteTournaments = validator.GetRecentIncompleteTournaments(_db);
            ParseTournaments(recentIncompleteTournaments, writer, validator);
        }

        private static void ParseTournaments(List<Tournament> tournaments, DataWriter writer, DataValidator validator)
        {
            foreach (var tournament in tournaments)
            {
                System.Console.WriteLine("Recording Tournament: " + tournament.TournamentName);
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
            }
            System.Console.WriteLine("Done");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
