using System.ComponentModel.DataAnnotations;
using BlogBank.Api.Models;
using BlogBank.Core.Interfaces;
using BlogBank.Core.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogBank.Api.Controllers;

/// <summary>
/// 认证控制器，提供登录、刷新 Token 和登出接口。
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(IUserRepository userRepo, ITokenService tokenService) : ControllerBase
{
    /// <summary>用户登录，验证用户名和密码，返回双 Token。</summary>
    // POST /api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await userRepo.GetByUsernameAsync(req.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "用户名或密码错误。" });

        if (!user.IsEnabled)
            return Unauthorized(new { message = "账号已禁用。" });

        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
        var refreshToken = await tokenService.GenerateRefreshTokenAsync(user.Id);

        return Ok(new { accessToken, refreshToken, expiresAt });
    }

    /// <summary>使用 Refresh Token 换取新的双 Token（旧 Token 同时撤销）。</summary>
    // POST /api/auth/refresh
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        var userId = await tokenService.ValidateRefreshTokenAsync(req.RefreshToken);
        if (userId is null)
            return Unauthorized(new { message = "Refresh Token 无效或已过期。" });

        var user = await userRepo.GetByIdAsync(userId.Value);
        if (user is null || !user.IsEnabled)
            return Unauthorized(new { message = "用户不存在或已禁用。" });

        await tokenService.RevokeRefreshTokenAsync(req.RefreshToken);

        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
        var newRefreshToken = await tokenService.GenerateRefreshTokenAsync(user.Id);

        return Ok(new { accessToken, refreshToken = newRefreshToken, expiresAt });
    }

    /// <summary>登出，撤销 Refresh Token。</summary>
    // POST /api/auth/logout
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
    {
        await tokenService.RevokeRefreshTokenAsync(req.RefreshToken);
        return NoContent();
    }

    private void SetRefreshTokenCookie(string token, DateTime expiry)
    {
        Response.Cookies.Append(ConstString.RefreshTokenCookie,token,new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = new DateTimeOffset(expiry,TimeSpan.Zero),
            Path = "/api/auth"
        });
    }
}

public record RefreshRequest([Required] string RefreshToken);
