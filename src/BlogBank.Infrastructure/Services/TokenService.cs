using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace BlogBank.Infrastructure.Services;

public class TokenService(IConfiguration configuration, IConnectionMultiplexer? redis, IMemoryCache memory) : ITokenService
{
    private bool UseRedis => redis is { IsConnected: true };

    public (string token, DateTime expiresAt) GenerateAccessToken(User user)
    {
        var secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        var issuer     = configuration["Jwt:Issuer"] ?? "BlogBank";
        var audience   = configuration["Jwt:Audience"] ?? "BlogBank";
        var expMinutes = int.TryParse(configuration["Jwt:AccessTokenExpireMinutes"], out var m) ? m : 15;

        var key       = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds     = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(expMinutes);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("Nickname", user.Nickname),
            new Claim("UserId",user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("tokenVersion",$"{user.TokenVersion + 1}".ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public async Task<string> GenerateRefreshTokenAsync(long userId)
    {
        var expDays = int.TryParse(configuration["Jwt:RefreshTokenExpireDays"], out var d) ? d : 7;
        var token   = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var cacheKey = $"refresh_token:{token}";
        var expiry   = TimeSpan.FromDays(expDays);

        if (UseRedis)
        {
            try
            {
                await redis!.GetDatabase().StringSetAsync(cacheKey, userId.ToString(), expiry);
                return token;
            }
            catch { /* fall through */ }
        }

        memory.Set(cacheKey, userId.ToString(), expiry);
        return token;
    }

    public async Task<long?> ValidateRefreshTokenAsync(string refreshToken)
    {
        var cacheKey = $"refresh_token:{refreshToken}";

        if (UseRedis)
        {
            try
            {
                var value = await redis!.GetDatabase().StringGetAsync(cacheKey);
                if (value.HasValue)
                    return long.TryParse(value, out var uid) ? uid : null;
            }
            catch { /* fall through */ }
        }

        return memory.TryGetValue(cacheKey, out string? val) && long.TryParse(val, out var userId)
            ? userId
            : null;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var cacheKey = $"refresh_token:{refreshToken}";

        if (UseRedis)
        {
            try
            {
                await redis!.GetDatabase().KeyDeleteAsync(cacheKey);
                return;
            }
            catch { /* fall through */ }
        }

        memory.Remove(cacheKey);
    }
}
