using BlogBank.Core.Entities;
using BlogBank.Core.Enums;

namespace BlogBank.Core.Interfaces;

/// <summary>
/// 工作 Bug 仓储接口，定义对 <see cref="WorkBug"/> 的持久化操作。
/// </summary>
public interface IWorkBugRepository
{
    /// <summary>
    /// 按查询条件获取 bug 列表，按发生日期倒序排列；所有条件均为可选，全部为空时返回全量。
    /// </summary>
    /// <param name="status">处理状态筛选，null 表示不过滤。</param>
    /// <param name="severity">严重程度筛选，null 表示不过滤。</param>
    /// <param name="project">所属项目/模块精确匹配，null 或空表示不过滤。</param>
    /// <param name="keyword">关键字，模糊匹配标题/现象/处理方法/原因分析，null 或空表示不过滤。</param>
    /// <returns>bug 集合。</returns>
    Task<IEnumerable<WorkBug>> GetAllAsync(
        BugStatusEnum? status = null,
        BugSeverityEnum? severity = null,
        string? project = null,
        string? keyword = null);

    /// <summary>
    /// 按 ID 查询单条 bug 记录。
    /// </summary>
    /// <param name="id">bug 记录的雪花 ID。</param>
    /// <returns>匹配的记录；若不存在则返回 <see langword="null"/>。</returns>
    Task<WorkBug?> GetByIdAsync(long id);

    /// <summary>
    /// 新增一条 bug 记录，ID 由雪花算法在仓储内部生成。
    /// </summary>
    /// <param name="bug">待新增的实体（无需设置 <see cref="WorkBug.Id"/>）。</param>
    /// <returns>已赋予 ID 的实体。</returns>
    Task<WorkBug> CreateAsync(WorkBug bug);

    /// <summary>
    /// 全量更新指定 ID 的 bug 记录。
    /// </summary>
    /// <param name="id">目标记录的雪花 ID。</param>
    /// <param name="bug">携带最新数据的实体。</param>
    /// <returns>更新后的实体；若 ID 不存在则返回 <see langword="null"/>。</returns>
    Task<WorkBug?> UpdateAsync(long id, WorkBug bug);

    /// <summary>
    /// 删除指定 ID 的 bug 记录。
    /// </summary>
    /// <param name="id">目标记录的雪花 ID。</param>
    /// <returns>删除成功返回 <see langword="true"/>；ID 不存在返回 <see langword="false"/>。</returns>
    Task<bool> DeleteAsync(long id);
}
