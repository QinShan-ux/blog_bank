using System.Text.Json;
using BlogBank.Core.Entities.Mq;
using BlogBank.Infrastructure.Entities;
using BlogBank.Infrastructure.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BlogBank.Api.Job;

public abstract class RabbitMqConsumer<T> : BackgroundService where T : MyMessage
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMessagePublisher _publisher;
    private readonly IConnection _connection;

    // 子类指定队列名
    protected virtual string QueueName => "export";
    protected virtual string ExchangeName => "export.exchange";
    protected virtual string RouteKey => "export.job";

    // 子类指定并发数
    protected virtual ushort PrefetchCount => 10;

    protected RabbitMqConsumer(ILogger logger,IServiceScopeFactory serviceScopeFactory,IMessagePublisher publisher,IConnection connection)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _publisher = publisher;
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        
        await channel.BasicQosAsync(0, PrefetchCount, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            T? message = null;
            try
            {
                message = JsonSerializer.Deserialize<T>(ea.Body.ToArray());
                if (message is null) throw new InvalidOperationException("消息反序列化失败");

                // 幂等检查（子类可重写）
                if (await IsDuplicateAsync(message.MessageId))
                {
                    _logger.LogWarning("重复消息，跳过 | MessageId={MessageId}", message.MessageId);
                    await channel.BasicAckAsync(ea.DeliveryTag, false,stoppingToken);
                    return;
                }

                await HandleAsync(message, stoppingToken);
                await channel.BasicAckAsync(ea.DeliveryTag, false,stoppingToken);

                _logger.LogInformation("消息处理成功 | MessageId={MessageId}", message.MessageId);
            }
            catch (Exception ex)
            {
                var messageId = message?.MessageId ?? "unknown";
                _logger.LogError(ex, "消息处理失败 | MessageId={MessageId}", messageId);


                // 重试次数未超限则重新入队，否则进死信队列
                if (message is not null && message.RetryCount < 3)
                {
                    message.RetryCount++;
                    // TODO: 重新发布带 RetryCount 的消息
                    await _publisher.PublishAsync(ExchangeName, RouteKey,message,stoppingToken);
                    
                    await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    return;
                }

                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false,stoppingToken);
            }
        };

        try
        {
            await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception e)
        {
            Console.WriteLine($"\n队列错误，可能{QueueName}队列不存在\n");
            throw;
        }
        
    }

    // 子类实现业务逻辑
    protected abstract Task HandleAsync(T message, CancellationToken ct);

    // 子类可重写：接入 Redis 等实现幂等
    protected virtual Task<bool> IsDuplicateAsync(string messageId) => Task.FromResult(false);
}