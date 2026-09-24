using Microsoft.Extensions.Caching.Memory;

namespace BlogBank.Api.Middlewares;

public class BlackListMiddleware(RequestDelegate next,IMemoryCache cache)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // SignalR 路径跳过检查
        if (context.Request.Path.StartsWithSegments("/hubs"))
        {
            await next(context);
            return;
        }
        
        var path = context.Request.Path;
        if (!path.Equals("/api/auth/login"))
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            var user = context.User;
            var revoke = user.FindFirst("revoke")?.Value;
            if (revoke == null || bool.Parse(revoke))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        
            var seq = user.FindFirst("seq")?.Value;
            int.TryParse(seq,out var seqValue);
            var key = user.FindFirst("UserId")?.Value;
            var seqMemory = cache.Get<long>($"blank:token:user:{key}");
            if (seqMemory > seqValue)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }
        await next(context);
    }
}