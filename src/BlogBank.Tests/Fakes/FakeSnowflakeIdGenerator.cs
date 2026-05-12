using BlogBank.Core.Interfaces;

namespace BlogBank.Tests.Fakes;

public class FakeSnowflakeIdGenerator : ISnowflakeIdGenerator
{
    private long _current = 1000;
    public long NextId() => Interlocked.Increment(ref _current);
}