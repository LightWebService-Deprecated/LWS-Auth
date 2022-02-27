using System.Threading.Tasks;
using LWS_Auth.Configuration;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace LWS_Auth.Repository;

/// <summary>
/// Mongo Context. Should be registered with 'Singleton' Object.
/// </summary>
public class MongoContext
{
    /// <summary>
    /// Mongo Client object, can access whole db or cluster itself.
    /// </summary>
    public readonly MongoClient MongoClient;

    /// <summary>
    /// Mongo Database Object, responsible for database itself.
    /// </summary>
    public readonly IMongoDatabase MongoDatabase;

    public readonly MongoConfiguration MongoConfiguration;

    public MongoContext(MongoConfiguration mongoConfiguration)
    {
        // Setup MongoDB Naming Convention
        var camelCase = new ConventionPack {new CamelCaseElementNameConvention()};
        ConventionRegistry.Register("CamelCase", camelCase, a => true);

        MongoConfiguration = mongoConfiguration;
        MongoClient = new MongoClient(mongoConfiguration.MongoConnection);
        MongoDatabase = MongoClient.GetDatabase(mongoConfiguration.MongoDbName);
    }
}