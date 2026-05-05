using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogBank.Infrastructure.Repositories;

/// <summary>
/// 操作日志仓储实现，基于 EF Core 对 <see cref="AuditLog"/> 进行持久化操作。
/// </summary>
public class AuditLogRepository(AppDbContext db) : IAuditLogRepository
{
    /// <summary>
    /// 分页查询操作日志，支持按操作人、操作类型、表名、时间范围过滤。
    /// </summary>
    public async Task<(IEnumerable<AuditLog> Items, int Total)> GetPagedAsync(
        int page, int pageSize,
        string? userId = null,
        string? action = null,
        string? tableName = null,
        DateTime? startTime = null,
        DateTime? endTime = null)
    {
        var query = db.AuditLogs.AsNoTracking();

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(l => l.UserId == userId);
        if (!string.IsNullOrEmpty(action))
            query = query.Where(l => l.Action == action);
        if (!string.IsNullOrEmpty(tableName))
            query = query.Where(l => l.TableName == tableName);
        if (startTime.HasValue)
            query = query.Where(l => l.OperatedAt >= startTime.Value);
        if (endTime.HasValue)
            query = query.Where(l => l.OperatedAt <= endTime.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.OperatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    /// <summary>
    /// 按 ID 查询单条操作日志。
    /// </summary>
    public async Task<AuditLog?> GetByIdAsync(int id)
        => await db.AuditLogs.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);

    /// <summary>
    /// 新增操作日志，操作时间由服务端自动记录。
    /// </summary>
    public async Task<AuditLog> CreateAsync(AuditLog log)
    {
        log.OperatedAt = DateTime.Now;
        db.AuditLogs.Add(log);
        await db.SaveChangesAsync();
        return log;
    }

    /// <summary>
    /// 删除指定 ID 的操作日志。
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var log = await db.AuditLogs.FindAsync(id);
        if (log is null) return false;

        db.AuditLogs.Remove(log);
        await db.SaveChangesAsync();
        return true;
    }
}
