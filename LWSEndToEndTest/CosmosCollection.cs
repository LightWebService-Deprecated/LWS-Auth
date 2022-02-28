using Xunit;

namespace LWSEndToEndTest;

[CollectionDefinition("Cosmos")]
public class CosmosCollection : ICollectionFixture<CosmosFixture>
{
}