using BlogBank.Core.Entities.Mq;
using Microsoft.Extensions.Logging;

namespace BlogBank.Infrastructure.Services;

/// <summary>
/// ConnectMq=false 时的 IMessagePublisher 空实现：
/// 不连接 RabbitMQ，发布动作仅记录警告，保证应用在无 Broker 环境可正常启动与运行。
/// </summary>
public class NullMessagePublisher(ILogger<NullMessagePublisher> logger) : IMessagePublisher
{
    public Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken ct = default)
        where T : MyMessage
    {
        logger.LogWarning(
            "RabbitMQ 未启用（ConnectMq=false），消息未发布 | Exchange={Exchange} RoutingKey={RoutingKey} MessageId={MessageId}",
            exchange, routingKey, message.MessageId);
        return Task.CompletedTask;
    }
}
