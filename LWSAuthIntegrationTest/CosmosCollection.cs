using Xunit;

namespace LWSAuthIntegrationTest;

[CollectionDefinition("Cosmos")]
public class CosmosCollection : ICollectionFixture<CosmosFixture>
{
}