namespace BlogBank.Core.Entities.Mq;

public abstract class MyMessage
{
    /// <summary>消息唯一ID，用于幂等去重</summary>
    public string MessageId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>消息创建时间</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>消息类型（自动取类名）</summary>
    public string MessageType => GetType().Name;

    /// <summary>重试次数</summary>
    public int RetryCount { get; set; } = 0;
}