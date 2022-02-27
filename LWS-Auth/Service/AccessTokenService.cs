using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LWS_Auth.Models;
using LWS_Auth.Repository;

namespace LWS_Auth.Service;

public class AccessTokenService
{
    private readonly IAccessTokenRepository _accessTokenRepository;

    public AccessTokenService(IAccessTokenRepository accessTokenRepository)
    {
        _accessTokenRepository = accessTokenRepository;
    }

    public async Task<AccessToken> CreateAccessTokenAsync(string userId)
    {
        using var shaManaged = new SHA512Managed();
        var targetString = $"{DateTime.UtcNow.Ticks}/{userId}/{Guid.NewGuid().ToString()}";
        var targetByte = Encoding.UTF8.GetBytes(targetString);
        var result = shaManaged.ComputeHash(targetByte);

        var accessToken = new AccessToken
        {
            Id = BitConverter.ToString(result).Replace("-", string.Empty),
            UserId = userId
        };

        await _accessTokenRepository.InsertAccessTokenAsync(accessToken);

        return accessToken;
    }
}