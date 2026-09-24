using BlogBank.Core.Entities;

namespace BlogBank.Core.Interfaces;

public interface ITokenService
{
    Task<(string token, DateTime expiresAt)> GenerateAccessToken(User user);
    Task<string> GenerateRefreshTokenAsync(User user);
    Task<long?> ValidateRefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);

    Task ClearToken(string accessToken, string refreshToken);
}
