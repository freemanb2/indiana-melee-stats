using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Demo.GraphQL;

namespace Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddConferenceClient()
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://workshop.chillicream.com/graphql"));

            IServiceProvider services = serviceCollection.BuildServiceProvider();

            IConferenceClient client = services.GetRequiredService<IConferenceClient>();

            var result = await client.GetSessions.ExecuteAsync();
            //result.EnsureNoErrors();

            foreach (var session in result.Data.Sessions.Nodes)
            {
                Console.WriteLine(session.Title);
            }
        }
    }
}
