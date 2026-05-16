using System.Text.Json;
using BlogBank.Api.Tool;
using BlogBank.Infrastructure.Entities;
using BlogBank.Infrastructure.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BlogBank.Api.Job;

public class ExportWorker: RabbitMqConsumer<ExportJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public ExportWorker(ILogger<ExportWorker> logger, IServiceScopeFactory serviceScopeFactory,IMessagePublisher publisher,IConnection connection) 
        : base(logger, serviceScopeFactory,publisher,connection)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override string QueueName => "export";
    protected override string ExchangeName => "export.exchange";
    
    protected override string RouteKey => "export.exchange";


    protected override async Task HandleAsync(ExportJob message, CancellationToken ct)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var messageService = scope.ServiceProvider.GetRequiredService<ExportProcessor>();
        await messageService.ProcessAsync(message);
        Console.WriteLine(JsonSerializer.Serialize(message));

    }
}