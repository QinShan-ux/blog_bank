using BlogBank.Core.dto;
using BlogBank.Core.Entities;

namespace BlogBank.Core.Interfaces;

/// <summary>
/// 文章仓储接口，定义对 <see cref="Article"/> 的持久化操作。
/// </summary>
public interface IArticleRepository
{
    /// <summary>
    /// 获取所有文章，按发布日期倒序排列。
    /// </summary>
    /// <returns>文章集合。</returns>
    Task<IEnumerable<Article>> GetAllAsync();

    /// <summary>
    /// 按 ID 查询单篇文章。
    /// </summary>
    /// <param name="id">文章的雪花 ID。</param>
    /// <returns>匹配的文章；若不存在则返回 <see langword="null"/>。</returns>
    Task<Article?> GetByIdAsync(long id);

    /// <summary>
    /// 新增一篇文章，ID 由雪花算法在仓储内部生成。
    /// </summary>
    /// <param name="article">待新增的文章实体（无需设置 <see cref="Article.Id"/>）。</param>
    /// <returns>已赋予 ID 的文章实体。</returns>
    Task<Article> CreateAsync(Article article);

    /// <summary>
    /// 全量更新指定 ID 的文章内容及标签。
    /// </summary>
    /// <param name="id">目标文章的雪花 ID。</param>
    /// <param name="article">携带最新数据的文章实体。</param>
    /// <returns>更新后的文章实体；若 ID 不存在则返回 <see langword="null"/>。</returns>
    Task<Article?> UpdateAsync(long id, Article article);

    /// <summary>
    /// 删除指定 ID 的文章及其所有标签。
    /// </summary>
    /// <param name="id">目标文章的雪花 ID。</param>
    /// <returns>删除成功返回 <see langword="true"/>；ID 不存在返回 <see langword="false"/>。</returns>
    Task<bool> DeleteAsync(long id);

    /// <summary>
    /// 批量新增文章，所有记录在同一事务中写入。
    /// </summary>
    /// <param name="articles">待新增的文章实体集合（无需设置 <see cref="Article.Id"/>）。</param>
    /// <returns>已赋予 ID 的文章实体列表。</returns>
    Task<List<Article>> CreateBatchAsync(IEnumerable<Article> articles);

    /// <summary>
    /// 获取所有文章的 ID 和标题，按发布日期倒序排列（不加载正文等大字段）。
    /// </summary>
    /// <returns>包含 Id 和 Title 的轻量列表。</returns>
    Task<List<ArticleSummary>> GetIdTitleListAsync();
}
