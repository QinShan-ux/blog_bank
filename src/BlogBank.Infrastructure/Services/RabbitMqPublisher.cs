using System.Text.Json;
using BlogBank.Core.Entities.Mq;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace BlogBank.Infrastructure.Services;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken ct = default) where T : MyMessage;
}
 
public class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IChannel _channel;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
 
    public RabbitMqPublisher(
        IConnection connection,
        ILogger<RabbitMqPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }
 
    // 懒加载 Channel，避免启动时就创建
    private async Task<IChannel> GetChannelAsync(CancellationToken ct)
    {
        if (_channel is { IsOpen: true })
            return _channel;

        await _semaphore.WaitAsync(ct);
        try
        {
            if (_channel is { IsOpen: true })
                return _channel;
            
            // v7.1.0 在创建 Channel 时直接开启 Publisher Confirms
            _channel = await _connection.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled:        false,
                    publisherConfirmationTrackingEnabled: true,
                    outstandingPublisherConfirmationsRateLimiter: null,
                    consumerDispatchConcurrency: null
                ),
                cancellationToken: ct);

            _channel.BasicAcksAsync  += OnBasicAckAsync;
            _channel.BasicNacksAsync += OnBasicNackAsync;
            _channel.BasicReturnAsync += OnBasicReturnAsync;

            _logger.LogInformation("RabbitMQ Publisher Channel 已创建");
        }
        finally
        {
            _semaphore.Release();
        }

        return _channel;
    }

    private Task OnBasicAckAsync(object sender, BasicAckEventArgs ea)
    {
        _logger.LogDebug("Broker 已确认消息 DeliveryTag={DeliveryTag}", ea.DeliveryTag);
        return Task.CompletedTask;
    }

    private Task OnBasicNackAsync(object sender, BasicNackEventArgs ea)
    {
        _logger.LogError("Broker 拒绝消息 DeliveryTag={DeliveryTag}", ea.DeliveryTag);
        return Task.CompletedTask;
    }
    
    private Task OnBasicReturnAsync(object sender, BasicReturnEventArgs e)
    {
        // e.ReplyCode  : AMQP 错误码，如 312 = NO_ROUTE，313 = NO_CONSUMERS
        // e.ReplyText  : 错误描述
        // e.Exchange   : 目标 Exchange
        // e.RoutingKey : 路由键
        // e.Body       : 原始消息体（可反序列化回 MessageId）

        string? messageId = null;
        try
        {
            var message = JsonSerializer.Deserialize<MyMessage>(e.Body.Span);
            messageId = message?.MessageId;
        }
        catch { /* 反序列化失败不影响主流程 */ }

        _logger.LogWarning(
            "消息被退回 | Exchange={Exchange} RoutingKey={RoutingKey} " +
            "ReplyCode={ReplyCode} ReplyText={ReplyText} MessageId={MessageId}",
            e.Exchange, e.RoutingKey, e.ReplyCode, e.ReplyText, messageId);

        // 根据业务需要决定后续处理：
        // 1. 记录日志告警（当前做法）
        // 2. 写入数据库补偿表
        // 3. 发送到备用队列

        return Task.CompletedTask;
    }
    public async Task PublishAsync<T>(
        string exchange,
        string routingKey,
        T message,
        CancellationToken ct = default) where T : MyMessage
    {
        _logger.LogInformation(
            "发布消息 | Exchange={Exchange} RoutingKey={RoutingKey} MessageId={MessageId}",
            exchange, routingKey, message.MessageId);
        var channel = await GetChannelAsync(ct);

        var body  = JsonSerializer.SerializeToUtf8Bytes(message);
        var props = new BasicProperties
        {
            Persistent  = true,
            MessageId   = message.MessageId,
            ContentType = "application/json",
            Timestamp   = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            Headers     = new Dictionary<string, object?>
            {
                ["x-retry-count"]  = message.RetryCount,
                ["x-published-at"] = DateTime.UtcNow.ToString("O")
            }
        };

        try
        {
            await channel.BasicPublishAsync(
                exchange:        exchange,
                routingKey:      routingKey,
                mandatory:       true,
                basicProperties: props,
                body:            body,
                cancellationToken: ct);
            _logger.LogInformation(
                "消息发布成功 | Exchange={Exchange} RoutingKey={RoutingKey} MessageId={MessageId}",
                exchange, routingKey, message.MessageId);
        }
        catch (PublishException ex)
        {
            // 打出详细原因
            _logger.LogError(ex,
                "发布失败 | Exchange={Exchange} RoutingKey={RoutingKey} | {Detail}",
                exchange, routingKey, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "消息发布失败 | Exchange={Exchange} RoutingKey={RoutingKey} MessageId={MessageId}",
                exchange, routingKey, message.MessageId);
            throw;
        }
    }
 
    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }
 
        _semaphore.Dispose();
    }
}