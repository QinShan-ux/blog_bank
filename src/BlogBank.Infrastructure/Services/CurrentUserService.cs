using BlogBank.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BlogBank.Infrastructure.Services;

public class CurrentUserService: ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // 从 JWT Token 中获取当前登录用户信息
    public string UserId => _httpContextAccessor.HttpContext?
        .User.FindFirst("UserId")?.Value;

    public string UserName => _httpContextAccessor.HttpContext?
        .User.Identity?.Name;
}