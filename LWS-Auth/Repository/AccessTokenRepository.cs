using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LWS_Auth.Configuration;
using LWS_Auth.Extension;
using LWS_Auth.Models;
using Microsoft.Azure.Cosmos;

namespace LWS_Auth.Repository;

public interface IAccessTokenRepository
{
    Task InsertAccessTokenAsync(AccessToken accessToken);
    Task<AccessToken> GetAccessTokenByTokenAsync(string token);
    Task<List<AccessToken>> ListAccessTokensAsync(string userId);
}

public class AccessTokenRepository : IAccessTokenRepository
{
    private readonly Container _accessTokenContainer;
    private IQueryable<AccessToken> AccessTokenQueryable => _accessTokenContainer.GetItemLinqQueryable<AccessToken>();

    public AccessTokenRepository(CosmosClient cosmosClient, CosmosConfiguration cosmosConfiguration)
    {
        _accessTokenContainer = cosmosClient.GetContainer(cosmosConfiguration.CosmosDbname,
            cosmosConfiguration.AccessTokenContainerName);
    }

    public async Task InsertAccessTokenAsync(AccessToken accessToken)
    {
        await _accessTokenContainer.CreateItemAsync(accessToken, new PartitionKey(accessToken.UserId));
    }

    public async Task<AccessToken> GetAccessTokenByTokenAsync(string token)
    {
        return await AccessTokenQueryable.Where(a => a.Id == token)
            .CosmosFirstOrDefaultAsync();
    }

    public async Task<List<AccessToken>> ListAccessTokensAsync(string userId)
    {
        return await AccessTokenQueryable.Where(a => a.UserId == userId)
            .CosmosToListAsync();
    }
}