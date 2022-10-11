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
            
            foreach (var tournament in validTournaments)
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
