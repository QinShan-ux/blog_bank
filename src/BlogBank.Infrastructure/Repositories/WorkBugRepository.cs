using BlogBank.Core.Entities;
using BlogBank.Core.Enums;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogBank.Infrastructure.Repositories;

/// <summary>
/// 工作 Bug 仓储实现，基于 EF Core 对 <see cref="WorkBug"/> 进行持久化操作。
/// </summary>
public class WorkBugRepository(AppDbContext db, ISnowflakeIdGenerator idGen) : IWorkBugRepository
{
    /// <summary>
    /// 按查询条件获取 bug 列表，按发生日期倒序排列；条件全部为空时返回全量。
    /// </summary>
    public async Task<IEnumerable<WorkBug>> GetAllAsync(
        BugStatusEnum? status = null,
        BugSeverityEnum? severity = null,
        string? project = null,
        string? keyword = null)
    {
        var query = db.WorkBugs.AsNoTracking();

        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);
        if (severity.HasValue)
            query = query.Where(b => b.Severity == severity.Value);
        if (!string.IsNullOrWhiteSpace(project))
            query = query.Where(b => b.Project == project);
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(b =>
                b.Title.Contains(keyword) ||
                b.Description.Contains(keyword) ||
                b.Solution.Contains(keyword) ||
                (b.RootCause != null && b.RootCause.Contains(keyword)));

        return await query
            .OrderByDescending(b => b.OccurredDate)
            .ToListAsync();
    }

    /// <summary>
    /// 按 ID 查询单条 bug 记录。
    /// </summary>
    public async Task<WorkBug?> GetByIdAsync(long id)
    {
        return await db.WorkBugs
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    /// <summary>
    /// 新增 bug 记录，由雪花算法生成 ID。
    /// </summary>
    public async Task<WorkBug> CreateAsync(WorkBug bug)
    {
        bug.Id = idGen.NextId();
        db.WorkBugs.Add(bug);
        await db.SaveChangesAsync();
        return bug;
    }

    /// <summary>
    /// 全量更新 bug 记录字段。
    /// </summary>
    public async Task<WorkBug?> UpdateAsync(long id, WorkBug updated)
    {
        // 使用AsNoTracking 不创建快照 不进行追踪
        var existing = await db.WorkBugs
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (existing is null) return null;

        // 把未追踪的实体"挂载"回 DbContext，设置状态为已修改
        db.WorkBugs.Attach(existing).State = EntityState.Modified;
        existing.Title        = updated.Title;
        existing.Description  = updated.Description;
        existing.RootCause    = updated.RootCause;
        existing.Solution     = updated.Solution;
        existing.Severity     = updated.Severity;
        existing.Status       = updated.Status;
        existing.Project      = updated.Project;
        existing.Environment  = updated.Environment;
        existing.OccurredDate = updated.OccurredDate;
        existing.SolvedDate   = updated.SolvedDate;

        await db.SaveChangesAsync();
        return existing;
    }

    /// <summary>
    /// 删除指定 bug 记录。
    /// </summary>
    public async Task<bool> DeleteAsync(long id)
    {
        var bug = await db.WorkBugs.FindAsync(id);
        if (bug is null) return false;

        db.WorkBugs.Remove(bug);
        await db.SaveChangesAsync();
        return true;
    }
}
