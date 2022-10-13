using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IO;

using System.Collections.Generic;
using System;

namespace Elo_Calculator
{
    class Program
    {
        static void Main(string[] args)
        {
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, ".env");
            DotEnv.Load(dotenv);

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            MongoClient dbClient = new MongoClient(config["MONGODB_PATH"]);
            IMongoDatabase _db = dbClient.GetDatabase("IndianaMeleeStatsDB");

            var _sets = _db.GetCollection<BsonDocument>("Sets");
            var _players = _db.GetCollection<BsonDocument>("Players");

            var unprocessedSets = _sets.Find(x => x["Processed"] == false).ToList();

            UpdateRatings(unprocessedSets, _sets, _players);
        }

        private static void UpdateRatings(List<BsonDocument> unprocessedSets, IMongoCollection<BsonDocument> _sets, IMongoCollection<BsonDocument> _players)
        {
            foreach (var set in unprocessedSets)
            {
                var player1id = set["Players"][0]["_id"];
                var player2id = set["Players"][1]["_id"];

                var player1 = _players.Find(x => x["_id"] == player1id).Single().ToBsonDocument();
                var player2 = _players.Find(x => x["_id"] == player2id).Single().ToBsonDocument();

                int D = Math.Abs(player1.GetValue("Elo").AsInt32 - player2.GetValue("Elo").AsInt32);

                //Todo - get pd from table
                Tuple<double, double> scoringProbability = GetScoringProbability(D);
                double PDH = scoringProbability.Item1;
                double PDL = scoringProbability.Item2;

                // Determine the points
                string displayScore = set["DisplayScore"].AsString;
                int totalGames = set["TotalGames"].AsInt32;
                double winnerPoints = 0;
                double loserPoints = 0;

                switch (totalGames)
                {
                    case 3:
                        if (displayScore.EndsWith("1") || displayScore.EndsWith("2"))
                        // Bo3
                        {
                            winnerPoints = 0.75;
                            loserPoints = 0.25;
                        }
                        else
                        // Bo5
                        {
                            winnerPoints = 1.0;
                        }
                        break;
                    case 4:
                        winnerPoints = 0.85;
                        loserPoints = 0.15;
                        break;
                    case 5:
                        winnerPoints = 0.7;
                        loserPoints = 0.3;
                        break;
                    default:
                        break;
                }

                // Scale points by PD values
                double player1Points = 0;
                double player2Points = 0;
                if (set["WinnerId"] == player1["_id"])
                {
                    if (player1["Elo"] >= player2["Elo"])
                    {
                        // Player 1 won and was higher rated
                        player1Points = (1 - PDH) * winnerPoints;
                        player2Points = (1 - PDL) * loserPoints;
                    }
                    else
                    {
                        // Player 1 won and was lower rated
                        player1Points = (1 - PDL) * winnerPoints;
                        player2Points = (1 - PDH) * loserPoints;
                    }

                }
                else if (player1["Elo"] >= player2["Elo"])
                {
                    // Player 2 won and was lower rated
                    player1Points = (1 - PDH) * loserPoints;
                    player2Points = (1 - PDL) * winnerPoints;
                }
                else
                {
                    // Player 2 won and was higher rated
                    player1Points = (1 - PDL) * loserPoints;
                    player2Points = (1 - PDH) * winnerPoints;
                }

                // Determine scaling coefficient for each player
                int K1 = 20;
                if (player1["Elo"] >= 2400) K1 = 10;
                else if (_sets.CountDocuments(x => x["_id"] == player1["_id"]) < 30) K1 = 40;

                int K2 = 20;
                if (player2["Elo"] >= 2400) K2 = 10;
                else if (_sets.CountDocuments(x => x["_id"] == player2["_id"]) < 30) K2 = 40;

                // Calculate new ratings
                int player1RatingChange = (int)Math.Ceiling(player1Points * K1);
                int player2RatingChange = (int)Math.Ceiling(player2Points * K2);

                int player1Rating = player1["Elo"].AsInt32 + player1RatingChange;
                int player2Rating = player2["Elo"].AsInt32 + player2RatingChange;

                // Update ratings
                var update = Builders<BsonDocument>.Update
                    .Set(p => p["Elo"], player1Rating);
                _players.UpdateOne(x => x["_id"] == player1["_id"], update);

                update = Builders<BsonDocument>.Update
                    .Set(p => p["Elo"], player2Rating);
                _players.UpdateOne(x => x["_id"] == player2["_id"], update);

                // Mark set as processed
                update = Builders<BsonDocument>.Update
                    .Set(p => p["Processed"], true);
                _sets.UpdateOne(x => x["_id"] == set["_id"], update);
            }
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
