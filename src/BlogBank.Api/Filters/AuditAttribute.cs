using System.Security.Claims;
using BlogBank.Core.dto;
using BlogBank.Core.Entities;
using BlogBank.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BlogBank.Api.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AuditAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        // 把 HTTP 层信息存入 HttpContext.Items
        // ChangeTracker 保存时会从这里取
        context.HttpContext.Items["AuditInfo"] = new AuditHttpInfo
        {
            // 追踪ID：唯一标识这次请求，关联 HTTP 层和数据层的日志
            TraceId    = context.HttpContext.TraceIdentifier,

            // 从 JWT Token 获取当前登录用户
            UserId     = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            UserName   = context.HttpContext.User.Identity?.Name,

            // 请求信息
            RequestUrl = context.HttpContext.Request.Path,
            HttpMethod = context.HttpContext.Request.Method,
            IpAddress  = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
            OperatedAt = DateTime.Now
        };

        // 执行接口
        await next();
    }
}