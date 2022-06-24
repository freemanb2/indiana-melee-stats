using API_Scraper.API;
using GraphQL;
using GraphQL.Client.Abstractions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        public async Task<List<Tournament>> GetRecentIndianaTournamentResults()
        {
            var query = new GraphQLRequest
            {
                Query = @"
                query RecentIndianaTournamentResults {
                    tournaments(query: { filter: { addrState: ""IN"", videogameIds: [1], past: true, published: true, publiclySearchable: true }, perPage: 1, page: 1 }){
                        nodes {
                            id
                            name
                            events(filter: { type: 1 }) {
                                id
                                name
                                type
                                tournament {
                                    id
                                }
                                sets (page: 1, perPage: 100) {
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