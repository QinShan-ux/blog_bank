using System.IdentityModel.Tokens.Jwt;
using BlogBank.Core.Interfaces;

namespace BlogBank.Api.Middlewares;

public class WhitelistMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICacheService redis)
    {
        // SignalR 路径跳过检查
        if (context.Request.Path.StartsWithSegments("/hubs"))
        {
            await next(context);
            return;
        }
        var path = context.Request.Path;
        if(!path.Equals("/api/auth/login"))
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                var exists = await redis.IsExit($"access_token:{jti}");
                if (!exists)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { message = "请重新登录" });
                    return;
                }
            }
        }

        await next(context);
    }
}