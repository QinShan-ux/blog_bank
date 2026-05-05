using BlogBank.Core.Entities;

namespace BlogBank.Core.Interfaces;

public interface ITokenService
{
    (string token, DateTime expiresAt) GenerateAccessToken(User user);
    Task<string> GenerateRefreshTokenAsync(long userId);
    Task<long?> ValidateRefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}
