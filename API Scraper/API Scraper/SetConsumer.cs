using API_Scraper.API;
using GraphQL;
using GraphQL.Client.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var response = await _client.SendQueryAsync<GetAllSetsResponse>(query);
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
    }

    public class GetAllSetsResponse
    {
        [JsonProperty("player")]
        public GetSetsResponse Response { get; set; }
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