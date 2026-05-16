namespace BlogBank.Infrastructure.Entities;

public class RabbitMqOptions
{
    public string Host     { get; set; }
    public int    Port     { get; set; } = 5672;
    public string UserName { get; set; }
    public string Password { get; set; }

    public List<QueueOptions> Queues { get; set; } = new();
}

public class QueueOptions
{
    public string Exchange       { get; set; }
    public string DeadExchange   { get; set; }
    public string Queue          { get; set; }
    public string DeadQueue      { get; set; }
    public string RoutingKey     { get; set; }
    public string DeadRoutingKey { get; set; }
}