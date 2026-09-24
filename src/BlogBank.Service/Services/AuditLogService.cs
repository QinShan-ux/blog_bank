using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Service.Interfaces;

namespace BlogBank.Service.Services;

public class AuditLogService(IAuditLogRepository repo) : IAuditLogService
{
    public Task<(IEnumerable<AuditLog> Items, int Total)> GetPagedAsync(
        int page, int pageSize,
        string? userId = null,
        string? action = null,
        string? tableName = null,
        DateTime? startTime = null,
        DateTime? endTime = null)
        => repo.GetPagedAsync(page, pageSize, userId, action, tableName, startTime, endTime);

    public Task<AuditLog?> GetByIdAsync(long id) => repo.GetByIdAsync(id);

    public Task<AuditLog> CreateAsync(AuditLog log) => repo.CreateAsync(log);

    public Task<bool> DeleteAsync(long id) => repo.DeleteAsync(id);
}
