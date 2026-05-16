using BlogBank.Core.Enums;
using BlogBank.Core.Interfaces;
using BlogBank.Service.Interfaces;

namespace BlogBank.Service.Services;

public class AuthService(
    IUserRepository userRepo,
    ITokenService tokenService,
    ICacheService cache) : IAuthService
{
    public async Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)?> LoginAsync(
        string username, string password)
    {
        var user = await userRepo.GetByUsernameAsync(username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        if (!user.IsEnabled)
            return null;

        var newVersion = user.TokenVersion + 1;
        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
        var refreshToken = await tokenService.GenerateRefreshTokenAsync(user.Id);
        await cache.SetAsync($"tokenVersion:{user.Id}", newVersion.ToString(), 30, TimeEnum.Day);
        _ = userRepo.UpdateVersion(user.Id, newVersion);

        return (accessToken, refreshToken, expiresAt);
    }

    public async Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)?> RefreshAsync(
        string refreshToken)
    {
        var userId = await tokenService.ValidateRefreshTokenAsync(refreshToken);
        if (userId is null)
            return null;

        var user = await userRepo.GetByIdAsync(userId.Value);
        if (user is null || !user.IsEnabled)
            return null;

        await tokenService.RevokeRefreshTokenAsync(refreshToken);

        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
        var newRefreshToken = await tokenService.GenerateRefreshTokenAsync(user.Id);

        return (accessToken, newRefreshToken, expiresAt);
    }

    public Task LogoutAsync(string refreshToken) => tokenService.RevokeRefreshTokenAsync(refreshToken);
}
