// using System;
// using System.Collections.Generic;
// using System.Net;
// using System.Net.Http;
// using System.Threading;
// using System.Threading.Tasks;
// using LWS_Auth.Configuration;
// using LWS_Auth.Repository;
// using Xunit;
//
// namespace LWSEndToEndTest;
//
// public class Test : IDisposable
// {
//     private readonly DockerHelper _dockerHelper;
//     private readonly MongoConfiguration _mongoConfiguration;
//     private readonly MongoContext _mongoContext;
//
//     private readonly string _connectionString;
//
//     public Test()
//     {
//         _dockerHelper = new DockerHelper();
//         _mongoConfiguration = new MongoConfiguration
//         {
//             MongoConnection = Environment.GetEnvironmentVariable("E2E_MONGODB_CONNECTION"),
//             MongoDbName = Guid.NewGuid().ToString()
//         };
//
//         _mongoContext = new MongoContext(_mongoConfiguration);
//
//         _dockerHelper.CreateContainerAsync(new Dictionary<string, object>
//         {
//             ["IsEncrypted"] = false,
//             ["Values"] = new Dictionary<string, object>()
//             {
//                 ["AzureWebJobsStorage"] = "UseDevelopmentStorage=true",
//                 ["FUNCTIONS_WORKER_RUNTIME"] = "dotnet-isolated",
//                 ["MongoSection:MongoConnection"] = _mongoConfiguration.MongoConnection,
//                 ["MongoSection:MongoDbName"] = _mongoConfiguration.MongoDbName
//             }
//         }).Wait();
//
//         _connectionString = _dockerHelper.RunContainerAsync().GetAwaiter().GetResult();
//     }
//
//     [Fact]
//     public async Task test()
//     {
//         var httpClient = new HttpClient
//         {
//             BaseAddress = new Uri(_connectionString)
//         };
//
//         Thread.Sleep(10 * 1000);
//         var response = await httpClient.GetAsync("/api/misc/alive");
//
//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//     }
//
//     public void Dispose()
//     {
//         _dockerHelper.DestroyContainerAsync().Wait();
//         _dockerHelper.Dispose();
//         _mongoContext.MongoClient.DropDatabase(_mongoConfiguration.MongoDbName);
//     }
// }

