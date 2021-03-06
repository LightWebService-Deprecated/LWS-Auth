using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LWSAuthService.Models;
using LWSAuthService.Repository;
using LWSAuthServiceTest.Helpers;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Xunit;

namespace LWSAuthServiceTest.Repository;

[Collection("IntegrationCollections")]
public class AccessTokenRepositoryTest
{
    private readonly IMongoCollection<AccessToken> _accessTokenCollection;
    private readonly IAccessTokenRepository _accessTokenRepository;
    private IMongoQueryable<AccessToken> AccessTokenQueryable => _accessTokenCollection.AsQueryable();

    private AccessToken TestAccessToken(string token = "testToken") => new AccessToken
    {
        Id = token,
        UserId = "testUserId"
    };

    public AccessTokenRepositoryTest(IntegrationTestFixture integrationTestFixture)
    {
        var context = integrationTestFixture._MongoContext;
        _accessTokenCollection = context.AccessTokenCollection;
        _accessTokenRepository =
            new AccessTokenRepository(context);
    }

    private async Task<List<AccessToken>> ListAccessTokenAsync()
    {
        return await AccessTokenQueryable.ToListAsync();
    }

    [Fact(DisplayName =
        "InsertAccessTokenAsync: InsertAccessTokenAsync should insert one data if there is no duplicated result.")]
    public async Task Is_InsertAccessTokenAsync_Inserts_Data_If_No_Duplicates()
    {
        // Let
        var accessToken = TestAccessToken();

        // Do
        await _accessTokenRepository.InsertAccessTokenAsync(accessToken);

        // Check
        var list = await ListAccessTokenAsync();
        Assert.Single(list);

        var firstEntity = list.First();
        Assert.Equal(accessToken.Id, firstEntity.Id);
        Assert.Equal(accessToken.UserId, firstEntity.UserId);
    }

    [Fact(DisplayName =
        "GetAccessTokenByTokenAsync: GetAccessTokenByTokenAsync should return null if data does not exists.")]
    public async Task Is_GetAccessTokenByTokenAsync_Returns_Null_When_No_Data()
    {
        // let
        var token = "testToken";

        // Do
        var result = await _accessTokenRepository.GetAccessTokenByTokenAsync(token);

        // Check
        Assert.Null(result);
    }

    [Fact(DisplayName =
        "GetAccessTokenByTokenAsync: GetAccessTokenByTokenAsync should return corresponding data if data exists.")]
    public async Task Is_GetAccessTokenByTokenAsync_Returns_Corresponding_Data_Well()
    {
        // Let
        var accessToken = TestAccessToken();
        await _accessTokenCollection.InsertOneAsync(accessToken);

        // Do
        var result = await _accessTokenRepository.GetAccessTokenByTokenAsync(accessToken.Id);

        // Check
        Assert.NotNull(result);
        Assert.Equal(accessToken.Id, result.Id);
        Assert.Equal(accessToken.UserId, result.UserId);
    }

    [Fact(DisplayName = "ListAccessTokensAsync: ListAccessTokensAsync should return list of data if data exists.")]
    public async Task Is_ListAccessTokensAsync_Return_List_Of_Data_If_Data_Exists()
    {
        // Let
        var toSaveList = new List<AccessToken>
        {
            TestAccessToken(),
            TestAccessToken("hello"),
            TestAccessToken("another")
        };
        await _accessTokenCollection.InsertManyAsync(toSaveList);

        // Do
        var list = await _accessTokenRepository.ListAccessTokensAsync(TestAccessToken().UserId);

        // Check
        Assert.Equal(3, list.Count);
    }

    [Fact(DisplayName = "BulkRemoveAccessTokenAsync: BulkRemoveAccessTokenAsync should remove all account's token")]
    public async Task Is_BulkRemoveAccessTokenAsync_Removes_Account_Token()
    {
        // Let
        var toSaveList = new List<AccessToken>
        {
            TestAccessToken(),
            TestAccessToken("hello"),
            TestAccessToken("another")
        };
        await _accessTokenCollection.InsertManyAsync(toSaveList);

        // Do
        await _accessTokenRepository.BulkRemoveAccessTokenAsync("testUserId");

        // Check
        var list = await ListAccessTokenAsync();
        Assert.Empty(list);
    }
}