namespace BlogBank.Service.Interfaces;

public interface IAuthService
{
    Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)?> LoginAsync(string username, string password);
    Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)?> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
}
