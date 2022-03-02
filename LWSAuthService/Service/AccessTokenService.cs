using System.Security.Cryptography;
using System.Text;
using LWSAuthService.Models;
using LWSAuthService.Repository;

namespace LWSAuthService.Service;

public class AccessTokenService
{
    private readonly IAccessTokenRepository _accessTokenRepository;

    public AccessTokenService(IAccessTokenRepository accessTokenRepository)
    {
        _accessTokenRepository = accessTokenRepository;
    }

    public async Task<AccessToken> CreateAccessTokenAsync(string userId, HashSet<AccountRole> roles)
    {
        using var shaManaged = new SHA512Managed();
        var targetString = $"{DateTime.UtcNow.Ticks}/{userId}/{Guid.NewGuid().ToString()}";
        var targetByte = Encoding.UTF8.GetBytes(targetString);
        var result = shaManaged.ComputeHash(targetByte);

        var accessToken = new AccessToken
        {
            Id = BitConverter.ToString(result).Replace("-", string.Empty),
            UserId = userId,
            Roles = roles
        };

        await _accessTokenRepository.InsertAccessTokenAsync(accessToken);

        return accessToken;
    }
}