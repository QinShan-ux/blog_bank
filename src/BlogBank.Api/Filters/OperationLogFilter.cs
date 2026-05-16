using System.Diagnostics;
using System.Security.Claims;
using BlogBank.Core.Entities.Mq;
using BlogBank.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BlogBank.Api.Filters;

public class OperationLogFilter : IAsyncActionFilter
{
    private readonly IMessagePublisher _publisher;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILogger<OperationLogFilter> _logger;

    public OperationLogFilter(IMessagePublisher publisher,IHttpContextAccessor httpContext,ILogger<OperationLogFilter> logger)
    {
        _publisher = publisher;
        _httpContext = httpContext;
        _logger = logger;
    }
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next)
    {
        // 1. 记录请求信息（执行 Action 前）
        var requestBody = await ReadRequestBodyAsync(context.HttpContext.Request);

        // 2. 执行真正的 Action
        var executedContext = await next();

        // 3. Action 执行完后，异步发送审计消息，不阻塞响应
        _ = PublishAuditAsync(context, executedContext, requestBody);
    }
    private async Task PublishAuditAsync(
        ActionExecutingContext context,
        ActionExecutedContext executedContext,
        string requestBody)
    {
        try
        {
            // 只审计成功的请求，异常不记录（按需调整）
            if (executedContext.Exception != null) return;

            var httpContext = context.HttpContext;
            var user = httpContext.User;

            // 从 Attribute 上取业务描述
            var auditAttr = context.ActionDescriptor
                .EndpointMetadata
                .OfType<AuditAttribute>()
                .FirstOrDefault();

            var message = new AuditMessage
            {
                Action       = context.ActionDescriptor.DisplayName,
                OperatorId   = user.FindFirst("sub")?.Value,
                OperatorName = user.FindFirst("name")?.Value,
                RequestPath  = httpContext.Request.Path,
                RequestMethod= httpContext.Request.Method,
                RequestBody  = requestBody,
                ResponseCode = httpContext.Response.StatusCode,
                ClientIp     = httpContext.Connection.RemoteIpAddress?.ToString(),
                TraceId      = Activity.Current?.TraceId.ToString(),
                OccurredAt   = DateTime.UtcNow
            };

            await _publisher.PublishAsync("audit.exchange", "audit.log",message);
        }
        catch (Exception ex)
        {
            // 审计失败不能影响业务，只记日志
            _logger.LogError(ex, "审计消息发送失败");
        }
    }

    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        // Body 默认不可重复读，需要先开启
        request.EnableBuffering();
        request.Body.Position = 0;

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        request.Body.Position = 0; // 读完归位，后续 Action 还需要读
        return body;
    }
}
public class AuditMessage : MyMessage
{
    public string Action { get; set; }
    public string OperatorId { get; set; }
    public string OperatorName { get; set; }
    public string RequestPath { get; set; }
    public string RequestMethod { get; set; }
    public string RequestBody { get; set; }
    public int ResponseCode { get; set; }
    public string ClientIp { get; set; }
    public string TraceId { get; set; }
    public DateTime OccurredAt { get; set; }
}