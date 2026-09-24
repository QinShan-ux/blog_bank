using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BlogBank.Core.Entities;
using BlogBank.Core.Enums;
using BlogBank.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace BlogBank.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ICacheService _cache;
    private readonly IMemoryCache _memoryCache;
    private readonly String _secretKey;
    private readonly String _issuer;
    private readonly String _audience;
    private readonly int _expMinutes;
    private readonly int _expDay;

    public TokenService(IConfiguration configuration, ICacheService cache, IMemoryCache memoryCache)
    {
        _configuration = configuration;
        _cache = cache;
        _memoryCache = memoryCache;
        _secretKey = _configuration["Jwt:SecretKey"]
                     ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        _issuer = _configuration["Jwt:Issuer"] ?? "BlogBank";
        _audience   = _configuration["Jwt:Audience"] ?? "BlogBank";
        _expMinutes = int.Parse(_configuration["Jwt:AccessTokenExpireMinutes"] ?? "30");
        _expDay = int.Parse(_configuration["Jwt:RefreshTokenExpireDays"] ?? "7");
    }
    public async Task<(string token, DateTime expiresAt)> GenerateAccessToken(User user)
    {
        var key       = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds     = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var accessExpire = DateTime.UtcNow.AddMinutes(_expMinutes);
        // 从redis中获取序列号 + 1
        var seq = await _cache.Incr($"blank:token:user:{user.Id}");
        // 将序列号保存到本地缓存
        _memoryCache.Set($"blank:token:user:{user.Id}", seq);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Account),
            new Claim("Nickname", user.Nickname),
            new Claim("UserId",user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("tokenVersion",$"{user.TokenVersion + 1}".ToString()),
            new Claim("revoke","false"),
            new Claim("seq",seq.ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             _issuer,
            audience:           _audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            accessExpire,
            signingCredentials: creds);
        var resToken = new JwtSecurityTokenHandler().WriteToken(token);
        var id = new JwtSecurityTokenHandler().ReadJwtToken(resToken).Id;
        // var cacheKey = $"access_token:{id}";
        // cache.SetAsync(cacheKey, user.Id.ToString(), expMinutes, TimeEnum.Minute);
        return (resToken, accessExpire);
    }

    public async Task<string> GenerateRefreshTokenAsync(User user)
    {
        var key       = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds     = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        var refreshExpire = DateTime.UtcNow.AddMinutes(_expMinutes);
        var cacheKey = $"refresh_token:{user.Id}";
        var token = new JwtSecurityToken(
            issuer:             _issuer,
            audience:           _audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            refreshExpire,
            signingCredentials: creds);
        var resToken = new JwtSecurityTokenHandler().WriteToken(token);
        await _cache.SetAsync(cacheKey, resToken, 7, TimeEnum.Day);
        return resToken;
    }

    public async Task<long?> ValidateRefreshTokenAsync(string refreshToken)
    {
        var cacheKey = $"refresh_token:{refreshToken}";

        var value = await _cache.GetAsync(cacheKey);
        if(!string.IsNullOrEmpty(value))
            return long.TryParse(value, out var uid) ? uid : null;
        return null;
        // if (UseRedis)
        // {
        //     try
        //     {
        //         var value = await redis!.GetDatabase().StringGetAsync(cacheKey);
        //         if (value.HasValue)
        //             return long.TryParse(value, out var uid) ? uid : null;
        //     }
        //     catch { /* fall through */ }
        // }

        // return memory.TryGetValue(cacheKey, out string? val) && long.TryParse(val, out var userId)
        //     ? userId
        //     : null;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var cacheKey = $"refresh_token:{refreshToken}";

        await _cache.RemoveAsync(cacheKey);
        // if (UseRedis)
        // {
        //     try
        //     {
        //         await redis!.GetDatabase().KeyDeleteAsync(cacheKey);
        //         return;
        //     }
        //     catch { /* fall through */ }
        // }
        //
        // memory.Remove(cacheKey);
    }

    public Task ClearToken(string accessToken, string refreshToken)
    {
        var id = new JwtSecurityTokenHandler().ReadJwtToken(accessToken).Id;
        var accessCache = $"access_token:{id}";
        var refreshCache = $"refresh_token:{refreshToken}";
        return _cache.RemoveAsync(accessCache, refreshCache);
    }
}
