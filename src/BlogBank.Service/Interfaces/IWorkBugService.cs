using BlogBank.Core.Entities;
using BlogBank.Core.Enums;

namespace BlogBank.Service.Interfaces;

public interface IWorkBugService
{
    Task<IEnumerable<WorkBug>> GetAllAsync(
        BugStatusEnum? status = null,
        BugSeverityEnum? severity = null,
        string? project = null,
        string? keyword = null);
    Task<WorkBug?> GetByIdAsync(long id);
    Task<WorkBug> CreateAsync(WorkBug bug);
    Task<WorkBug?> UpdateAsync(long id, WorkBug bug);
    Task<bool> DeleteAsync(long id);
}
