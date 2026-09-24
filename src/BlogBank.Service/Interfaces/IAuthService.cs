namespace BlogBank.Service.Interfaces;

public interface IAuthService
{
    Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)?> LoginAsync(string account, string password);
    Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)?> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken,string accessToken);
}
