using BlogBank.Core.Entities;

namespace BlogBank.Core.Interfaces;

public interface IAuditLogRepository
{
    Task<(IEnumerable<AuditLog> Items, int Total)> GetPagedAsync(
        int page, int pageSize,
        string? userId = null,
        string? action = null,
        string? tableName = null,
        DateTime? startTime = null,
        DateTime? endTime = null);

    Task<AuditLog?> GetByIdAsync(int id);
    Task<AuditLog> CreateAsync(AuditLog log);
    Task<bool> DeleteAsync(int id);
}
