using BlogBank.Core.Interfaces;

namespace BlogBank.Infrastructure.Common;

/// <summary>
/// Twitter Snowflake 算法实现。
/// ID 结构（64 bit）：
///   [0]        1  bit  符号位，固定为 0
///   [1..41]   41  bit  毫秒时间戳（相对自定义 Epoch）
///   [42..51]  10  bit  机器 ID（0 ~ 1023）
///   [52..63]  12  bit  序列号（0 ~ 4095，同毫秒内自增）
/// </summary>
public sealed class SnowflakeIdGenerator : ISnowflakeIdGenerator
{
    // 自定义纪元：2024-01-01 00:00:00 UTC
    private static readonly long Epoch =
        new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();

    private const int SequenceBits   = 12;
    private const int MachineIdBits  = 10;
    private const long MaxSequence   = (1L << SequenceBits) - 1;   // 4095
    private const long MaxMachineId  = (1L << MachineIdBits) - 1;  // 1023
    private const int MachineIdShift = SequenceBits;                // 12
    private const int TimestampShift = SequenceBits + MachineIdBits; // 22

    private readonly long _machineId;
    private long _lastTimestamp = -1L;
    private long _sequence = 0L;
    private readonly Lock _lock = new();

    public SnowflakeIdGenerator(long machineId = 1)
    {
        if (machineId < 0 || machineId > MaxMachineId)
            throw new ArgumentOutOfRangeException(
                nameof(machineId), $"机器 ID 必须在 0 ~ {MaxMachineId} 之间。");
        _machineId = machineId;
    }

    public long NextId()
    {
        lock (_lock)
        {
            var timestamp = CurrentTimestamp();

            if (timestamp < _lastTimestamp)
                throw new InvalidOperationException("系统时钟发生回拨，拒绝生成 ID。");

            if (timestamp == _lastTimestamp)
            {
                _sequence = (_sequence + 1) & MaxSequence;
                if (_sequence == 0)
                    timestamp = WaitForNextMillis(_lastTimestamp);
            }
            else
            {
                _sequence = 0;
            }

            _lastTimestamp = timestamp;

            return ((timestamp - Epoch) << TimestampShift)
                 | (_machineId << MachineIdShift)
                 | _sequence;
        }
    }

    private static long CurrentTimestamp() =>
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    private static long WaitForNextMillis(long last)
    {
        var ts = CurrentTimestamp();
        while (ts <= last) ts = CurrentTimestamp();
        return ts;
    }
}
