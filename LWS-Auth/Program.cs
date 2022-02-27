using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LWS_Auth.Configuration;
using LWS_Auth.Middleware;
using LWS_Auth.Repository;
using LWS_Auth.Service;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace LWS_Auth
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults(a => a.UseMiddleware<AuthorizationMiddleware>())
                .ConfigureServices(SetupDependencyService)
                .Build();

            host.Run();
        }

        private static void SetupDependencyService(HostBuilderContext builderContext,
            IServiceCollection serviceCollection)
        {
            // Get IConfiguration Object
            var configuration = builderContext.Configuration;

            // Setup MongoDB Configuration
            var cosmosConfiguration = configuration.GetSection("CosmosSection").Get<CosmosConfiguration>();
            serviceCollection.AddSingleton(cosmosConfiguration);

            // Add Scoped Service(Mostly Business Logic)
            serviceCollection.AddScoped<AccountService>();
            serviceCollection.AddScoped<AccessTokenService>();

            // Add Singleton Service(Mostly Data Logic)
            serviceCollection.AddSingleton<CosmosClient>(CosmosClientHelper.CreateCosmosClient(cosmosConfiguration)
                .GetAwaiter().GetResult());
            serviceCollection.AddSingleton<IAccountRepository, AccountRepository>();
            serviceCollection.AddSingleton<IAccessTokenRepository, AccessTokenRepository>();
        }
    }
}