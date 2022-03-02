using LWSAuthService.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace LWSAuthService.Repository;

public interface IAccessTokenRepository
{
    Task InsertAccessTokenAsync(AccessToken accessToken);
    Task<AccessToken> GetAccessTokenByTokenAsync(string token);
    Task<List<AccessToken>> ListAccessTokensAsync(string userId);
}

public class AccessTokenRepository : IAccessTokenRepository
{
    private readonly IMongoCollection<AccessToken> _accessTokenCollection;
    private IMongoQueryable<AccessToken> AccessTokenQueryable => _accessTokenCollection.AsQueryable();

    public AccessTokenRepository(MongoContext mongoContext)
    {
        _accessTokenCollection = mongoContext.AccessTokenCollection;
    }

    public async Task InsertAccessTokenAsync(AccessToken accessToken)
    {
        await _accessTokenCollection.InsertOneAsync(accessToken);
    }

    public async Task<AccessToken> GetAccessTokenByTokenAsync(string token)
    {
        return await AccessTokenQueryable.Where(a => a.Id == token)
            .FirstOrDefaultAsync();
    }

    public async Task<List<AccessToken>> ListAccessTokensAsync(string userId)
    {
        return await AccessTokenQueryable.Where(a => a.UserId == userId)
            .ToListAsync();
    }
}