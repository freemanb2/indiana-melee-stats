using API_Scraper.API;
using GraphQL;
using GraphQL.Client.Abstractions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace API_Scraper
{
    public class TournamentHandler
    {
        private readonly IGraphQLClient _client;
        public TournamentHandler(IGraphQLClient client)
        {
            _client = client;
        }

        public async Task<Tournament> GetSpecificTournamentResults(string tournamentId)
        {
            Tournament tournament = new Tournament();
            int page = 1;
            int limit = 30;
            GraphQLResponse<GetSpecificIndianaTournamentResultsResponse> response = await GetNextTournamentResultsPage(tournamentId, page++, limit);
            tournament = response.Data.Tournament;
            if (tournament.Events.Any(e => e.Sets.Nodes.Count == limit))
            {
                response = await GetNextTournamentResultsPage(tournamentId, page++, limit);
                while (response.Data.Tournament.Events.Any(e => e.Sets.Nodes.Count != 0))
                {
                    foreach (var t in tournament.Events)
                    {
                        Event e = response.Data.Tournament.Events.Where(x => x.Name == t.Name).Single();
                        t.Sets.Nodes.AddRange(e.Sets.Nodes);
                    }
                    if (response.Data.Tournament.Events.All(e => e.Sets.Nodes.Count < limit))
                    {
                        break;
                    }
                    response = await GetNextTournamentResultsPage(tournamentId, page++, limit);
                }
            }
            return tournament;
        }

        private async Task<GraphQLResponse<GetSpecificIndianaTournamentResultsResponse>> GetNextTournamentResultsPage(string tournamentId, int page, int limit)
        {
            GraphQLResponse<GetSpecificIndianaTournamentResultsResponse> response;
            while (true)
            {
                try
                {
                    response = await _client.SendQueryAsync<GetSpecificIndianaTournamentResultsResponse>(GetSpecificTournamentQueryString(tournamentId, page++, limit));
                    break;
                }
                catch (GraphQL.Client.Http.GraphQLHttpRequestException e)
                {
                    System.Console.WriteLine("Too many requests exception detected, sleeping for 30 seconds....");
                    System.Console.WriteLine($"Exception details: {e.Message}");
                    System.Threading.Thread.Sleep(30000);
                    System.Console.WriteLine("Retrying request");
                }
            }
            return response;
        }

        public async Task<List<string>> GetRecentIndianaTournamentIds(int numTournaments)
        {
            var query = new GraphQLRequest
            {
                Query = @"
                    query RecentIndianaTournamentIds {
                      tournaments(query: { filter: { addrState: ""IN"", videogameIds: [1], past: true, published: true, publiclySearchable: true }, page: 1, perPage: " + numTournaments + @" }){
                        nodes {
                          id
                        }
                      }
                    }
                "
            };
            GraphQLResponse<GetRecentIndianaTournamentIdsResponse> response = await _client.SendQueryAsync<GetRecentIndianaTournamentIdsResponse>(query);

            var tournamentIds = new List<string>();
            var tournamentResults = response.Data.Tournaments.Nodes;

            foreach(var tournament in tournamentResults)
            {
                tournamentIds.Add(tournament.Id.ToString());
            }

            return tournamentIds;
        }

        public async Task<List<string>> GetRecentIndianaOnlineTournamentIds(List<Tuple<string, string>> validOnlineTournaments)
        {
            var tournamentIds = new List<string>();

            foreach (var onlineTournament in validOnlineTournaments)
            {
                var query = new GraphQLRequest
                {
                    Query = @"
                    query RecentIndianaOnlineTournamentIds {
                      tournaments(query: { filter: { videogameIds: [1], past: true, published: true, publiclySearchable: true, hasOnlineEvents: true, ownerId: " + onlineTournament.Item1 + @", name: """+ onlineTournament.Item2 + @""" }, page: 1, perPage: 500 }){
                        nodes {
                          id
                        }
                      }
                    }
                "
                };
                GraphQLResponse<GetRecentIndianaTournamentIdsResponse> response = await _client.SendQueryAsync<GetRecentIndianaTournamentIdsResponse>(query);

                var tournamentResults = response.Data.Tournaments.Nodes;

                foreach (var tournament in tournamentResults)
                {
                    tournamentIds.Add(tournament.Id.ToString());
                }
            }

            return tournamentIds;
        }

        public async Task<string> GetUserIdOfPlayer(string playerId)
        {
            var query = new GraphQLRequest
            {
                Query = @"
                    query GetUser {
                      player(id: " + playerId + @") {
                        user{
                          id
                        }
                      }
                    }
                "
            };
            GraphQLResponse<GetUserIdOfPlayerResponse> response = await _client.SendQueryAsync<GetUserIdOfPlayerResponse>(query);

            return response.Data.Player.User.Id;
        }

        private string GetSpecificTournamentQueryString(string tournamentId, int page, int limit)
        {
            return @"
                query SpecificIndianaTournamentResults {
                    tournament(id: " + tournamentId + @"){
                        id
                        name
                        slug
                        startAt
                        events(filter: { type: 1, videogameId: 1 }) {
                            id
                            name
                            type
                            state
                            tournament {
                                id
                            }
                            sets (page: " + page + @", perPage: " + limit + @", filters: { state: 3 }) {
                                nodes {
                                    id
                                    displayScore
                                    winnerId
                                    totalGames
                                    completedAt
                                    slots {
                                        standing {
                                            entrant {
                                                id
                                                participants {
                                                    player {
                                                        id
                                                        gamerTag
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }";
        }
    }

    public class GetRecentIndianaTournamentIdsResponse
    {
        public TournamentConnection Tournaments { get; set; }
    }

    public class GetSpecificIndianaTournamentResultsResponse
    {
        public Tournament Tournament { get; set; }
    }

    public class GetUserIdOfPlayerResponse
    {
        public Player Player { get; set; }
    }

}