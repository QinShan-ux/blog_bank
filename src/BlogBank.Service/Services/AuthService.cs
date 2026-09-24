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
        string account, string password)
    {
        var user = await userRepo.GetByUsernameAsync(account);
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        if (!user.IsEnabled)
            return null;

        
        var (accessToken, expiresAt) = await tokenService.GenerateAccessToken(user);
        // var (refreshToken, expiresAtRefresh) = await tokenService.GenerateAccessToken(user);
        var refreshToken = await tokenService.GenerateRefreshTokenAsync(user);

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

        var (accessToken, expiresAt) = await tokenService.GenerateAccessToken(user);
        var newRefreshToken = await tokenService.GenerateRefreshTokenAsync(user);

        return (accessToken, newRefreshToken, expiresAt);
    }

    public Task LogoutAsync(string refreshToken, string accessToken)
    {
        // 白名单方法，登出时直接将token删除
        // return  cache.RemoveAsync(new[] { accessToken, refreshToken });
        
        return tokenService.ClearToken(accessToken,refreshToken);
    }
}
