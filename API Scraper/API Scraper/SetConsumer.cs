using API_Scraper.API;
using GraphQL;
using GraphQL.Client.Abstractions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace API_Scraper
{
    public class SetConsumer
    {
        private readonly IGraphQLClient _client;
        public SetConsumer(IGraphQLClient client)
        {
            _client = client;
        }

        public async Task<List<Set>> GetAllSets(int playerId)
        {
            var query = new GraphQLRequest
            {
                Query = @"
                query Sets {
                  player(id: "+ playerId + @") {
                    id
                    sets(perPage: 10, page: 1) {
                      nodes {
                        id
                        displayScore
                        winnerId
                        slots {
                            id
                            entrant {
                                id
                                name
                            }
                        }
                        event {
                          id
                          name
                          tournament {
                            id
                            name
                          }
                        }
                      }
                    }
                  }
                }"
            };
            GraphQLResponse<GetAllSetsResponse> response = await _client.SendQueryAsync<GetAllSetsResponse>(query);
            System.Console.WriteLine(response.Data.Response.Sets.Nodes);
            return response.Data.Response.Sets.Nodes;
        }

        public async Task<List<Set>> GetHeadToHeadSets(int playerId1, int playerId2)
        {
            var query = new GraphQLRequest
            {
                Query = @"
                query Sets {
                  player(id: " + playerId1 + @") {
                    id
                    sets(perPage: 10, page: 1) {
                      nodes {
                        id
                        displayScore
                        winnerId
                        slots {
                            id
                            entrant(id: " + playerId2 + @") {
                                id
                                participants{
                                  player{
                                    id
                                    gamerTag
                                  }
                                }
                            }
                        }
                        event {
                          id
                          name
                          tournament {
                            id
                            name
                          }
                        }
                      }
                    }
                  }
                }"
            };
            var response = await _client.SendQueryAsync<GetAllSetsResponse>(query);
            return response.Data.Response.Sets.Nodes;
        }

        public async Task<Tournament> GetSpecificTournamentResults(string tournamentId)
        {
            Tournament tournament = new Tournament();
            int page = 1;
            int limit = 40;
            GraphQLResponse<GetSpecificIndianaTournamentResultsResponse> response = await _client.SendQueryAsync<GetSpecificIndianaTournamentResultsResponse>(GetSpecificTournamentQueryString(tournamentId, page++, limit));
            tournament = response.Data.Tournament;
            if (tournament.Events.Any(e => e.Sets.Nodes.Count == limit))
            {
                response = await _client.SendQueryAsync<GetSpecificIndianaTournamentResultsResponse>(GetSpecificTournamentQueryString(tournamentId, page++, limit));
                while (response.Data.Tournament.Events.Any(e => e.Sets.Nodes.Count != 0))
                {
                    foreach (var t in tournament.Events)
                    {
                        Event e = response.Data.Tournament.Events.Where(x => x.Name == t.Name).Single();
                        t.Sets.Nodes.AddRange(e.Sets.Nodes);
                    }
                    response = await _client.SendQueryAsync<GetSpecificIndianaTournamentResultsResponse>(GetSpecificTournamentQueryString(tournamentId, page++, limit));
                }
            }
            return tournament;
        }

        public async Task<List<Tournament>> GetRecentIndianaTournamentResults(int perPage = 1, int page = 1)
        {
            var query = new GraphQLRequest
            {
                Query = @"
                query RecentIndianaTournamentResults {
                    tournaments(query: { filter: { addrState: ""IN"", videogameIds: [1], past: true, published: true, publiclySearchable: true }, perPage: " + perPage + @", page: " + page + @" }){
                        nodes {
                            id
                            name
                            events(filter: { type: 1, videogameId: 1 }) {
                                id
                                name
                                type
                                state
                                tournament {
                                    id
                                }
                                sets (page: 1, perPage: 20, filters: { state: 3 }) {
                                  nodes {
                                    id
                                    displayScore
                                    winnerId
                                    slots {
                                      id
                                      seed {
                                        seedNum
                                      }
                                      standing {
                                        isFinal
                                        metadata
                                        placement
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
                    }
                }"
            };
            GraphQLResponse<GetRecentIndianaTournamentResultsResponse> response = await _client.SendQueryAsync<GetRecentIndianaTournamentResultsResponse>(query);
            return response.Data.Tournaments.Nodes;
        }

        public async Task<List<string>> GetRecentIndianaTournamentIds(int numTournaments = 1)
        {
            var query = new GraphQLRequest
            {
                Query = @"
                    query RecentIndianaTournamentIds {
                      tournaments(query: { filter: { addrState: ""IN"", videogameIds: [1], past: true, published: true, publiclySearchable: true }, page: 1, perPage:500 }){
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

        private string GetSpecificTournamentQueryString(string tournamentId, int page, int limit)
        {
            return @"
                query SpecificIndianaTournamentResults {
                    tournament(id: " + tournamentId + @"){
                        id
                        name
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

    public class GetAllSetsResponse
    {
        [JsonProperty("player")]
        public GetSetsResponse Response { get; set; }
    }

    public class GetRecentIndianaTournamentResultsResponse
    {
        public TournamentConnection Tournaments { get; set; }
    }

    public class GetRecentIndianaTournamentIdsResponse
    {
        public TournamentConnection Tournaments { get; set; }
    }

    public class GetSpecificIndianaTournamentResultsResponse
    {
        public Tournament Tournament { get; set; }
    }

}


//query RecentIndianaTournamentSets
//{
//    tournaments(query: { filter: { addrState: "IN", videogameIds: [1], past: true, published: true, publiclySearchable: true } }){
//        nodes {
//            id
//            name
//        events(filter: { videogameId: 1 }) {
//                entrants {
//                    nodes {
//                        id
//                        name
//                      standing {
//                            placement
//                            player{
//                                id
//                            }
//                        }
//                        paginatedSets(page: 1, perPage: 1){
//                            nodes {
//                                slots {
//                                    entrant {
//                                        id
//                                        name
//                                    }
//                                }
//                                displayScore
//                                winnerId
//                            }
//                        }
//                    }
//                }
//            }
//        }
//    }
//}