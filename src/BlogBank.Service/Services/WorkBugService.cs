using BlogBank.Core.Entities;
using BlogBank.Core.Enums;
using BlogBank.Core.Interfaces;
using BlogBank.Service.Interfaces;

namespace BlogBank.Service.Services;

public class WorkBugService(IWorkBugRepository repo) : IWorkBugService
{
    public Task<IEnumerable<WorkBug>> GetAllAsync(
        BugStatusEnum? status = null,
        BugSeverityEnum? severity = null,
        string? project = null,
        string? keyword = null)
        => repo.GetAllAsync(status, severity, project, keyword);

    public Task<WorkBug?> GetByIdAsync(long id) => repo.GetByIdAsync(id);

    public Task<WorkBug> CreateAsync(WorkBug bug) => repo.CreateAsync(bug);

    public Task<WorkBug?> UpdateAsync(long id, WorkBug bug) => repo.UpdateAsync(id, bug);

    public Task<bool> DeleteAsync(long id) => repo.DeleteAsync(id);
}
