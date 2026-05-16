using System.ComponentModel.DataAnnotations;
using BlogBank.Api.Models;
using BlogBank.Core.utils;
using BlogBank.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogBank.Api.Controllers;

/// <summary>
/// 认证控制器，提供登录、刷新 Token 和登出接口。
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>用户登录，验证用户名和密码，返回双 Token。</summary>
    // POST /api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var result = await authService.LoginAsync(req.Username, req.Password);
        if (result is null)
            return Unauthorized(new { message = "用户名或密码错误，或账号已禁用。" });

        var (accessToken, refreshToken, expiresAt) = result.Value;
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
        var result = await authService.RefreshAsync(req.RefreshToken);
        if (result is null)
            return Unauthorized(new { message = "Refresh Token 无效或已过期。" });

        var (accessToken, refreshToken, expiresAt) = result.Value;
        return Ok(new { accessToken, refreshToken, expiresAt });
    }

    /// <summary>登出，撤销 Refresh Token。</summary>
    // POST /api/auth/logout
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
    {
        await authService.LogoutAsync(req.RefreshToken);
        return NoContent();
    }
}

public record RefreshRequest([Required] string RefreshToken);
