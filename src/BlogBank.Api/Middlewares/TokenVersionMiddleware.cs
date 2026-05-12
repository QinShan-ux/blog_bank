using System.Security.Claims;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data;

namespace BlogBank.Api.Middlewares;

public class TokenVersionMiddleware(RequestDelegate next)
{
    
    public async Task InvokeAsync(HttpContext context, 
        AppDbContext db, ICacheService redis,
        ILogger<TokenVersionMiddleware> logger)
    {

        var path = context.Request.Path;
        // 如果是登录接口，不拦截
        if (!path.Equals("/api/auth/login"))
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tokenVersion = context.User.FindFirstValue("tokenVersion");
            if (string.IsNullOrEmpty(tokenVersion))
            {
                logger.LogError("❌ jwt中没有tokenVersion");
                return;
            }

            // 优先读 Redis
            var redisVersion = await redis.GetAsync($"tokenVersion:{userId}");
            var currentVersion = !string.IsNullOrEmpty(redisVersion)
                ? int.Parse(redisVersion)
                :  (await db.Users.FindAsync(userId))!.TokenVersion;

            if (currentVersion != int.Parse(tokenVersion))
            {
                context.Response.StatusCode = 401;
                logger.LogError("❌ tokenVersion 不一致");
                return;
            }
        }
        

        await next(context);
    }
}