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

        public async Task<List<Tournament>> GetSpecificTournamentResults(string tournamentId)
        {
          var query = new GraphQLRequest
          {
              Query = @"
              query RecentIndianaTournamentResults {
                tournaments(query: { filter: { addrState: ""IN"", videogameIds: [1], past: true, id: " + tournamentId + @" }, perPage: 1, page: 1 }){
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
                            sets (page: 1, perPage: 100, filters: { state: 3 }) {
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
                }
            }"
          };
          GraphQLResponse<GetRecentIndianaTournamentResultsResponse> response = await _client.SendQueryAsync<GetRecentIndianaTournamentResultsResponse>(query);
          return response.Data.Tournaments.Nodes;
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
                                sets (page: 1, perPage: 100, filters: { state: 3 }) {
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