using System.Diagnostics.CodeAnalysis;
using LWS_Auth.Configuration;
using LWS_Auth.Repository;
using LWS_Auth.Service;
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
                .ConfigureFunctionsWorkerDefaults()
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
            var mongoConfiguration = configuration.GetSection("MongoSection").Get<MongoConfiguration>();
            serviceCollection.AddSingleton(mongoConfiguration);

            // Add Scoped Service(Mostly Business Logic)
            serviceCollection.AddScoped<AccountService>();
            serviceCollection.AddScoped<AccessTokenService>();

            // Add Singleton Service(Mostly Data Logic)
            serviceCollection.AddSingleton<MongoContext>();
            serviceCollection.AddSingleton<IAccountRepository, AccountRepository>();
            serviceCollection.AddSingleton<IAccessTokenRepository, AccessTokenRepository>();
        }
    }
}