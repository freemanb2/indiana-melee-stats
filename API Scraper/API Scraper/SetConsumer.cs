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
                  player(id: "+ playerId +@") {
                    id
                    sets(perPage: 5, page: 10) {
                      nodes {
                        id
                        displayScore
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
            return response.Data.Player.Sets.Nodes;
        }
    }

    public class GetAllSetsResponse
    {
        public Player Player { get; set; }
    }
}
