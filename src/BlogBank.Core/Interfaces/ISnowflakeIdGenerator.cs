namespace BlogBank.Core.Interfaces;

/// <summary>
/// 雪花算法 ID 生成器接口。
/// 生成的 ID 为 64 位整数，具备时间有序、分布式唯一的特性。
/// </summary>
public interface ISnowflakeIdGenerator
{
    /// <summary>
    /// 生成下一个雪花 ID。
    /// </summary>
    /// <returns>64 位唯一整数 ID。</returns>
    /// <exception cref="InvalidOperationException">系统时钟发生回拨时抛出。</exception>
    long NextId();
}
