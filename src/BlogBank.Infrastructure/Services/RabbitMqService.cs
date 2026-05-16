using RabbitMQ.Client;

namespace BlogBank.Infrastructure.Services;

public class RabbitMqService
{
    // var factory = new ConnectionFactory
    // {
    //     HostName = "localhost",
    //     Port = 5672,
    //     UserName = "guest",
    //     Password = "guest",
    //     VirtualHost = "/"
    // };
    public required ConnectionFactory Factory;
    

    public RabbitMqService()
    {
        Factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/"
        };
    }

    public async Task SendMessage()
    {
        await using var connection = await Factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync("hello", durable: true, exclusive: false, autoDelete: false);
    }
    
}