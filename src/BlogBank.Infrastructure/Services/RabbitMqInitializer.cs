using BlogBank.Infrastructure.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlogBank.Infrastructure.Services;

using RabbitMQ.Client;

public class RabbitMqInitializer
{
    private readonly IConnection _connection;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqInitializer> _logger;

    public RabbitMqInitializer(
        IConnection connection,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqInitializer> logger)
    {
        _connection = connection;
        _options    = options.Value;
        _logger     = logger;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await using var channel = await _connection.CreateChannelAsync(cancellationToken: ct);

        foreach (var queue in _options.Queues)
        {
            await DeclareQueueAsync(channel, queue, ct);
            _logger.LogInformation("队列初始化完成 | Queue={Queue}", queue.Queue);
        }
    }

    private static async Task DeclareQueueAsync(
        IChannel channel, QueueOptions opt, CancellationToken ct)
    {
        // 主交换机
        await channel.ExchangeDeclareAsync(
            opt.Exchange, ExchangeType.Direct, durable: true, cancellationToken: ct);

        // 死信交换机
        await channel.ExchangeDeclareAsync(
            opt.DeadExchange, ExchangeType.Direct, durable: true, cancellationToken: ct);

        // 死信队列
        await channel.QueueDeclareAsync(
            opt.DeadQueue, durable: true, exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>{
                ["x-message-ttl"] = 7 * 24 * 60 * 60 * 1000
            },
            cancellationToken: ct);

        await channel.QueueBindAsync(
            opt.DeadQueue, opt.DeadExchange, opt.DeadRoutingKey, cancellationToken: ct);

        // 主队列
        await channel.QueueDeclareAsync(
            opt.Queue, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"]    = opt.DeadExchange,
                ["x-dead-letter-routing-key"] = opt.DeadRoutingKey
            },
            cancellationToken: ct);

        await channel.QueueBindAsync(
            opt.Queue, opt.Exchange, opt.RoutingKey, cancellationToken: ct);
    }
}