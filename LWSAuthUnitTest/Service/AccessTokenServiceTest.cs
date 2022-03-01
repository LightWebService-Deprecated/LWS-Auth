using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LWS_Auth.Models;
using LWS_Auth.Repository;
using LWS_Auth.Service;
using Moq;
using Xunit;

namespace LWSAuthUnitTest.Service;

public class AccessTokenServiceTest
{
    private AccessTokenService AccessTokenService => new AccessTokenService(_accessTokenRepository.Object);
    private readonly Mock<IAccessTokenRepository> _accessTokenRepository;

    public AccessTokenServiceTest()
    {
        _accessTokenRepository = new Mock<IAccessTokenRepository>();
    }

    [Fact(DisplayName =
        "CreateAccessTokenAsync: CreateAccessTokenAsync should create access token based on current tick")]
    public async Task Is_CreateAccessTokenAsync_Creates_Token_Based_On_Tick()
    {
        // Let
        var userId = "testUserId";
        _accessTokenRepository.Setup(a => a.InsertAccessTokenAsync(It.IsAny<AccessToken>()))
            .Callback((AccessToken accessToken) =>
            {
                Assert.NotNull(accessToken);
                Assert.NotNull(accessToken.Id);
                Assert.Equal(userId, accessToken.UserId);
                Assert.Single(accessToken.Roles);
                Assert.Equal(AccountRole.Admin, accessToken.Roles.First());
            });

        // Do
        await AccessTokenService.CreateAccessTokenAsync(userId, new HashSet<AccountRole> {AccountRole.Admin});

        // Verify
        _accessTokenRepository.VerifyAll();
    }
}