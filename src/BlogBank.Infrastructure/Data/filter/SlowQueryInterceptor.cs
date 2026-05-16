using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BlogBank.Infrastructure.Data.filter;

public class SlowQueryInterceptor: DbCommandInterceptor
{
    private static readonly TimeSpan _threshold = TimeSpan.FromMicroseconds(1);

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        if (eventData.Duration >= _threshold)
        {
            // 记录日志或告警
            Console.WriteLine($"[慢查询] {eventData.Duration.TotalMilliseconds}ms");
            Console.WriteLine(command.CommandText);
        }
        return result;
    }
}