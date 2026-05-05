using BlogBank.Core.Entities;

namespace BlogBank.Core.Interfaces;

/// <summary>
/// 随笔仓储接口，定义对 <see cref="Essay"/> 的持久化操作。
/// </summary>
public interface IEssayRepository
{
    /// <summary>
    /// 获取所有随笔，按发布日期倒序排列。
    /// </summary>
    /// <returns>随笔集合。</returns>
    Task<IEnumerable<Essay>> GetAllAsync();

    /// <summary>
    /// 按 ID 查询单篇随笔。
    /// </summary>
    /// <param name="id">随笔的雪花 ID。</param>
    /// <returns>匹配的随笔；若不存在则返回 <see langword="null"/>。</returns>
    Task<Essay?> GetByIdAsync(long id);

    /// <summary>
    /// 新增一篇随笔，ID 由雪花算法在仓储内部生成。
    /// </summary>
    /// <param name="essay">待新增的随笔实体（无需设置 <see cref="Essay.Id"/>）。</param>
    /// <returns>已赋予 ID 的随笔实体。</returns>
    Task<Essay> CreateAsync(Essay essay);

    /// <summary>
    /// 全量更新指定 ID 的随笔内容及标签。
    /// </summary>
    /// <param name="id">目标随笔的雪花 ID。</param>
    /// <param name="essay">携带最新数据的随笔实体。</param>
    /// <returns>更新后的随笔实体；若 ID 不存在则返回 <see langword="null"/>。</returns>
    Task<Essay?> UpdateAsync(long id, Essay essay);

    /// <summary>
    /// 删除指定 ID 的随笔及其所有标签。
    /// </summary>
    /// <param name="id">目标随笔的雪花 ID。</param>
    /// <returns>删除成功返回 <see langword="true"/>；ID 不存在返回 <see langword="false"/>。</returns>
    Task<bool> DeleteAsync(long id);

    /// <summary>
    /// 批量新增随笔，所有记录在同一事务中写入。
    /// </summary>
    /// <param name="essays">待新增的随笔实体集合（无需设置 <see cref="Essay.Id"/>）。</param>
    /// <returns>已赋予 ID 的随笔实体列表。</returns>
    Task<List<Essay>> CreateBatchAsync(IEnumerable<Essay> essays);
}
