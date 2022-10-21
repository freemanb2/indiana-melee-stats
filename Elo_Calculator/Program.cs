﻿using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IO;

using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Elo_Calculator
{
    public class Program
    {
        public static IMongoCollection<BsonDocument> _tournaments { get; set; }
        public static IMongoCollection<BsonDocument> _events { get; set; }
        public static IMongoCollection<BsonDocument> _sets { get; set; }
        public static IMongoCollection<BsonDocument> _players { get; set; }

        public static void Main()
        {
            InitializeDatabase();
            MarkStaleSets();
            RecalculateElos();
        }

        public static void UpdateRatingsWithSpecificSets(List<BsonDocument> setsToProcess)
        {
            InitializeDatabase();
            setsToProcess.Sort((x, y) => x["CompletedAt"].CompareTo(y["CompletedAt"]));
            setsToProcess.ForEach(x => x.Add("Processed", false));
            UpdateRatings(setsToProcess);
        }

        public static long MarkStaleSets()
        {
            InitializeDatabase();
            var update = Builders<BsonDocument>.Update
                    .Set(p => p["Stale"], true);
            var updatedCount = _sets.UpdateMany(x => x["CompletedAt"] < DateTime.Now.AddYears(-1) && x["Stale"] == false, update).ModifiedCount;

            return updatedCount;
        }

        public static void RecalculateElos()
        {
            ResetPlayerElos();
            var setsToProcess = GetRecentSets();
            UpdateRatings(setsToProcess);
        }

        private static void UpdateRatings(List<BsonDocument> setsToProcess)
        {
            var playerTournamentCounts = GetPlayerTournamentCounts();

            foreach (var set in setsToProcess)
            {
                var player1GamerTag = set["Players"][0]["GamerTag"].AsString;
                var player2GamerTag = set["Players"][1]["GamerTag"].AsString;

                BsonDocument player1;
                BsonDocument player2;

                FilterDefinition<BsonDocument> filter;

                try
                {
                    filter = Builders<BsonDocument>.Filter.Regex("GamerTag", new BsonRegularExpression("^"+Regex.Escape(player1GamerTag)+"$", "i"));
                    player1 = _players.Find(filter).Single().ToBsonDocument();
                }
                catch(Exception e)
                {
                    Console.WriteLine("Potential Start.gg name change detected - using Set Player._id instead");
                    player1 = _players.Find(x => x["_id"] == set["Players"][0]["_id"]).Single().ToBsonDocument();
                }

                try
                {
                    filter = Builders<BsonDocument>.Filter.Regex("GamerTag", new BsonRegularExpression("^"+Regex.Escape(player2GamerTag)+"$", "i"));
                    player2 = _players.Find(filter).Single().ToBsonDocument();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Potential Start.gg name change detected - using Set Player._id instead");
                    player2 = _players.Find(x => x["_id"] == set["Players"][1]["_id"]).Single().ToBsonDocument();
                }

                var minimumTournamentAttendance = 5;

                // Do not count elo changes for sets against low activity (usually out of state) players
                if (playerTournamentCounts.Where(x => x["GamerTag"].AsString.ToLower() == player1GamerTag.ToLower()).Single().GetValue("TournamentCount").AsInt32 < minimumTournamentAttendance) 
                    continue;

                if (playerTournamentCounts.Where(x => x["GamerTag"].AsString.ToLower() == player2GamerTag.ToLower()).Single().GetValue("TournamentCount").AsInt32 < minimumTournamentAttendance) 
                    continue;

                int D = Math.Abs(player1.GetValue("Elo").AsInt32 - player2.GetValue("Elo").AsInt32);

                Tuple<double, double> scoringProbability = GetScoringProbability(D);
                double PDH = scoringProbability.Item1;
                double PDL = scoringProbability.Item2;

                // Determine the points
                string displayScore = set["DisplayScore"].AsString;
                string player1SetCountString = displayScore.Substring(displayScore.IndexOf(" - ") - 1, 1);
                string player2SetCountString = displayScore.Substring(displayScore.Length - 1, 1);
                int player1SetCount = 0;
                int player2SetCount = 0;

                if (player1SetCountString == "W")
                {
                    player1SetCount = 2;
                    player2SetCount = 0;
                }
                else if (player2SetCountString == "W")
                {
                    player1SetCount = 0;
                    player2SetCount = 2;
                }
                else
                {
                    player1SetCount = int.Parse(player1SetCountString);
                    player2SetCount = int.Parse(player2SetCountString);
                }

                int setType = (player1SetCount == 3 || player2SetCount == 3) ? 5 : 3;
                int totalGames = player1SetCount + player2SetCount;

                double winnerPoints = 1;
                double loserPoints = 0;

                //switch (totalGames)
                //{
                //    case 2:
                //        // 2-0
                //        {
                //            winnerPoints = 1.0;
                //            break;
                //        }
                //    case 3:
                //        // 3-0 or 2-1
                //        if (setType == 3)
                //        // 2-1
                //        {
                //            winnerPoints = 0.85;
                //            loserPoints = 0.15;
                //        }
                //        else
                //        // 3-0
                //        {
                //            winnerPoints = 1.0;
                //        }
                //        break;
                //    case 4:
                //        // 3-1
                //        winnerPoints = 0.9;
                //        loserPoints = 0.1;
                //        break;
                //    case 5:
                //        // 3-2
                //        winnerPoints = 0.8;
                //        loserPoints = 0.2;
                //        break;
                //    default:
                //        break;
                //}

                // Scale points by PD values
                double player1Points = 0;
                double player2Points = 0;

                bool player1Won = ((setType == 5 && player1SetCount == 3) || (setType == 3 && player1SetCount == 2));

                if (player1Won)
                {
                    if (player1["Elo"] >= player2["Elo"])
                    {
                        // Player 1 won and was higher rated
                        player1Points = (winnerPoints - PDH);
                        player2Points = (loserPoints - PDL);
                    }
                    else
                    {
                        // Player 1 won and was lower rated
                        player1Points = (winnerPoints - PDL);
                        player2Points = (loserPoints - PDH);
                    }
                }
                else if (player1["Elo"] >= player2["Elo"])
                {
                    // Player 2 won and was lower rated
                    player1Points = (loserPoints - PDH);
                    player2Points = (winnerPoints - PDL);
                }
                else
                {
                    // Player 2 won and was higher rated
                    player1Points = (loserPoints - PDL);
                    player2Points = (winnerPoints - PDH);
                }

                // Determine scaling coefficient for each player
                int K1 = 20;
                if (player1["Elo"] >= 2400) K1 = 10;
                else if (setsToProcess.FindAll(x => x["Processed"] == true && x["Players"].AsBsonArray.Contains(player1["_id"])).Count < 10) K1 = 40;

                int K2 = 20;
                if (player2["Elo"] >= 2400) K2 = 10;
                else if (setsToProcess.FindAll(x => x["Processed"] == true && x["Players"].AsBsonArray.Contains(player2["_id"])).Count < 10) K2 = 40;

                // Scale down rating change if set was in-region
                double player1RegionalScale = 1.0;
                double player2RegionalScale = 1.0;

                //var player1RegionalOpponents = GetRegionalOpponents(player1);
                //if (player1RegionalOpponents.Contains(player2["GamerTag"].AsString))
                //{
                //    player1RegionalScale = 0.5;
                //}

                //var player2RegionalOpponents = GetRegionalOpponents(player2);
                //if (player2RegionalOpponents.Contains(player1["GamerTag"].AsString))
                //{
                //    player1RegionalScale = 0.5;
                //}

                var frequencyBias = GetFrequencyBias(player1, player2, setsToProcess);

                // Calculate new ratings
                int player1RatingChange = (int)Math.Round(player1Points * K1 * frequencyBias, 0, MidpointRounding.AwayFromZero);
                int player2RatingChange = (int)Math.Round(player2Points * K2 * frequencyBias, 0, MidpointRounding.AwayFromZero);

                int player1Rating = player1["Elo"].AsInt32 + player1RatingChange;
                int player2Rating = player2["Elo"].AsInt32 + player2RatingChange;

                // Update ratings
                var update = Builders<BsonDocument>.Update
                    .Set(p => p["Elo"], player1Rating);
                _players.UpdateOne(x => x["_id"] == player1["_id"], update);

                update = Builders<BsonDocument>.Update
                    .Set(p => p["Elo"], player2Rating);
                _players.UpdateOne(x => x["_id"] == player2["_id"], update);

                set.Set("Processed", true);
            }
        }

        private static double GetFrequencyBias(BsonDocument player1, BsonDocument player2, List<BsonDocument> setsToProcess)
        {
            double bias;

            var numSetsPlayed = setsToProcess.FindAll(x => x["Processed"] != false && x["Players"].AsBsonArray.Any(y => y["_id"] == player1["_id"]) && x["Players"].AsBsonArray.Any(y => y["_id"] == player2["_id"])).Count();

            switch (numSetsPlayed)
            {
                case var expression when numSetsPlayed <= 3:
                    bias = 1.0;
                    break;
                case var expression when numSetsPlayed <= 6:
                    bias = 0.95;
                    break;
                case var expression when numSetsPlayed <= 10:
                    bias = 0.85;
                    break;
                default:
                    bias = 0.35;
                    break;
            }

            return bias;
        }

        private static List<BsonDocument> GetPlayerTournamentCounts()
        {
            List<BsonDocument> players = _players.Find(x => true).ToList();
            
            players.ForEach((player) =>
            {
                var tournamentFilter = Builders<BsonDocument>.Filter.Regex("Events.Sets.Players.GamerTag", new BsonRegularExpression("^" + Regex.Escape(player["GamerTag"].AsString) + "$", "i"));
                var tournamentCount = _tournaments.Find(tournamentFilter).CountDocuments();
                player.Add("TournamentCount", (int)tournamentCount);
            });

            return players;
        }

        private static List<string> GetRegionalOpponents(BsonDocument player)
        {
            List<string> opponentTags = new List<string>();

            var filter = Builders<BsonDocument>.Filter.Eq("Players.Gamertag", player["GamerTag"]);
            var recentSets = _sets.Find(filter).SortByDescending(x => x["CompletedAt"]).ToList();
            var setCounts = new List<Tuple<string, int>>();
            foreach (var set in recentSets)
            {
                var opponentTag = set["Players"].AsBsonArray.Where(x => x["GamerTag"] != player["GamerTag"]).First().AsBsonDocument["GamerTag"];
                if (!setCounts.Any(x => x.Item1 == opponentTag.AsString))
                {
                    setCounts.Add(new Tuple<string, int>(opponentTag.AsString, 1));
                }
                else
                {
                    var index = setCounts.FindIndex(x => x.Item1 == opponentTag);
                    setCounts[index] = new Tuple<string, int>(opponentTag.AsString, setCounts[index].Item2 + 1);
                }
            }

            setCounts = setCounts.OrderByDescending(x => x.Item2).ToList();

            foreach(var opponent in setCounts)
            {
                if (opponent.Item2 >= 5) opponentTags.Add(opponent.Item1);

                if (opponentTags.Count() == 8) break;
            }

            return opponentTags;
        }

        private static void ResetPlayerElos()
        {
            var update = Builders<BsonDocument>.Update
                    .Set(p => p["Elo"], 1200);
            _players.UpdateMany(x => true, update);
        }

        private static List<BsonDocument> GetRecentSets()
        {
            List<BsonDocument> setsToProcess = _sets.Find(x => x["Stale"] == false).ToList();
            setsToProcess.Sort((x, y) => x["CompletedAt"].CompareTo(y["CompletedAt"]));
            setsToProcess.ForEach(x => x.Add("Processed", false));

            return setsToProcess;
        }

        private static void InitializeDatabase()
        {
            var cd = Environment.CurrentDirectory;
            var projectDirectory = Directory.GetParent(cd).Parent.Parent.FullName;
            var dotenv = Path.Combine(projectDirectory, ".env");
            DotEnv.Load(dotenv);

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            MongoClient dbClient = new MongoClient(config["MONGODB_PATH"]);
            IMongoDatabase _db = dbClient.GetDatabase("IndianaMeleeStatsDB");

            _tournaments = _db.GetCollection<BsonDocument>("Tournaments");
            _events = _db.GetCollection<BsonDocument>("Events");
            _sets = _db.GetCollection<BsonDocument>("Sets");
            _players = _db.GetCollection<BsonDocument>("Players");
        }

        private static Tuple<double, double> GetScoringProbability(int eloDiff)
        {
            switch (eloDiff)
            {
                case var expression when eloDiff <= 3:
                    return new Tuple<double, double>(0.50, 0.50);
                case var expression when eloDiff <= 10:
                    return new Tuple<double, double>(0.51, 0.49);
                case var expression when eloDiff <= 17:
                    return new Tuple<double, double>(0.52, 0.48);
                case var expression when eloDiff <= 25:
                    return new Tuple<double, double>(0.53, 0.47);
                case var expression when eloDiff <= 32:
                    return new Tuple<double, double>(0.54, 0.46);
                case var expression when eloDiff <= 39:
                    return new Tuple<double, double>(0.55, 0.45);
                case var expression when eloDiff <= 46:
                    return new Tuple<double, double>(0.56, 0.44);
                case var expression when eloDiff <= 53:
                    return new Tuple<double, double>(0.57, 0.43);
                case var expression when eloDiff <= 61:
                    return new Tuple<double, double>(0.58, 0.42);
                case var expression when eloDiff <= 68:
                    return new Tuple<double, double>(0.59, 0.41);
                case var expression when eloDiff <= 76:
                    return new Tuple<double, double>(0.60, 0.40);
                case var expression when eloDiff <= 83:
                    return new Tuple<double, double>(0.61, 0.39);
                case var expression when eloDiff <= 91:
                    return new Tuple<double, double>(0.62, 0.38);
                case var expression when eloDiff <= 98:
                    return new Tuple<double, double>(0.63, 0.37);
                case var expression when eloDiff <= 106:
                    return new Tuple<double, double>(0.64, 0.36);
                case var expression when eloDiff <= 113:
                    return new Tuple<double, double>(0.65, 0.35);
                case var expression when eloDiff <= 121:
                    return new Tuple<double, double>(0.66, 0.34);
                case var expression when eloDiff <= 129:
                    return new Tuple<double, double>(0.67, 0.33);
                case var expression when eloDiff <= 137:
                    return new Tuple<double, double>(0.68, 0.32);
                case var expression when eloDiff <= 145:
                    return new Tuple<double, double>(0.69, 0.31);
                case var expression when eloDiff <= 153:
                    return new Tuple<double, double>(0.70, 0.30);
                case var expression when eloDiff <= 162:
                    return new Tuple<double, double>(0.71, 0.29);
                case var expression when eloDiff <= 170:
                    return new Tuple<double, double>(0.72, 0.28);
                case var expression when eloDiff <= 179:
                    return new Tuple<double, double>(0.73, 0.27);
                case var expression when eloDiff <= 188:
                    return new Tuple<double, double>(0.74, 0.26);
                case var expression when eloDiff <= 197:
                    return new Tuple<double, double>(0.75, 0.25);
                case var expression when eloDiff <= 206:
                    return new Tuple<double, double>(0.76, 0.24);
                case var expression when eloDiff <= 215:
                    return new Tuple<double, double>(0.77, 0.23);
                case var expression when eloDiff <= 225:
                    return new Tuple<double, double>(0.78, 0.22);
                case var expression when eloDiff <= 235:
                    return new Tuple<double, double>(0.79, 0.21);
                case var expression when eloDiff <= 245:
                    return new Tuple<double, double>(0.80, 0.20);
                case var expression when eloDiff <= 256:
                    return new Tuple<double, double>(0.81, 0.19);
                case var expression when eloDiff <= 267:
                    return new Tuple<double, double>(0.82, 0.18);
                case var expression when eloDiff <= 278:
                    return new Tuple<double, double>(0.83, 0.17);
                case var expression when eloDiff <= 290:
                    return new Tuple<double, double>(0.84, 0.16);
                case var expression when eloDiff <= 302:
                    return new Tuple<double, double>(0.85, 0.15);
                case var expression when eloDiff <= 315:
                    return new Tuple<double, double>(0.86, 0.14);
                case var expression when eloDiff <= 328:
                    return new Tuple<double, double>(0.87, 0.13);
                case var expression when eloDiff <= 344:
                    return new Tuple<double, double>(0.88, 0.12);
                case var expression when eloDiff <= 357:
                    return new Tuple<double, double>(0.89, 0.11);
                case var expression when eloDiff <= 374:
                    return new Tuple<double, double>(0.90, 0.10);
                case var expression when eloDiff <= 391:
                    return new Tuple<double, double>(0.91, 0.09);
                default:
                    return new Tuple<double, double>(.92, .08);
            }
        }
    }
}
