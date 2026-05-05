using BlogBank.Core.dto;
using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogBank.Infrastructure.Repositories;

/// <summary>
/// 文章仓储实现，基于 EF Core 对 <see cref="Article"/> 进行持久化操作。
/// </summary>
public class ArticleRepository(AppDbContext db, ISnowflakeIdGenerator idGen) : IArticleRepository
{
    /// <summary>
    /// 获取所有文章及其标签，按发布日期倒序排列。
    /// </summary>
    public async Task<IEnumerable<Article>> GetAllAsync()
    {
        return await db.Articles
            .AsNoTracking()
            .Include(a => a.Tags)
            .OrderByDescending(a => a.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// 按 ID 查询单篇文章（含标签）。
    /// </summary>
    public async Task<Article?> GetByIdAsync(long id)
    {
        return await db.Articles
            .AsNoTracking()
            .Include(a => a.Tags)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <summary>
    /// 新增文章，由雪花算法生成 ID，并同步设置所有标签的外键。
    /// </summary>
    public async Task<Article> CreateAsync(Article article)
    {
        article.Id = idGen.NextId();
        foreach (var tag in article.Tags)
            tag.ArticleId = article.Id;

        db.Articles.Add(article);
        await db.SaveChangesAsync();
        return article;
    }

    /// <summary>
    /// 全量更新文章字段，先删除旧标签再批量插入新标签。
    /// </summary>
    public async Task<Article?> UpdateAsync(long id, Article updated)
    {
        // 使用AsNoTracking 取消追踪，跳过快照和状态管理
        var existing = await db.Articles
            .AsNoTracking()
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (existing is null) return null;
        // 把未追踪的实体"挂载"回 DbContext，设置状态为已修改
        db.Articles.Attach(existing).State = EntityState.Modified;
        existing.Title    = updated.Title;
        existing.Date     = updated.Date;
        existing.Category = updated.Category;
        existing.ReadTime = updated.ReadTime;
        existing.Excerpt  = updated.Excerpt;
        existing.Content  = updated.Content;

        // 删除旧标签，重新写入新标签
        existing.Tags = updated.Tags
            .Select(t => new ArticleTag { ArticleId = id, Tag = t.Tag })
            .ToList();

        await db.SaveChangesAsync();
        return existing;
    }

    /// <summary>
    /// 删除指定文章，级联删除由数据库约束处理。
    /// </summary>
    public async Task<bool> DeleteAsync(long id)
    {
        var article = await db.Articles.FindAsync(id);
        if (article is null) return false;

        db.Articles.Remove(article);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 仅查询 ID 和标题，通过列级投影避免加载正文等大字段。
    /// </summary>
    public async Task<List<ArticleSummary>> GetIdTitleListAsync()
    {
        return await db.Articles
            .AsNoTracking()
            .OrderByDescending(a => a.Date)
            .Select(a => new ArticleSummary(a.Id,a.Title))
            .ToListAsync();
    }

    /// <summary>
    /// 批量新增文章，为每条记录生成雪花 ID，一次 SaveChanges 写入。
    /// </summary>
    public async Task<List<Article>> CreateBatchAsync(IEnumerable<Article> articles)
    {
        var list = articles.ToList();
        foreach (var article in list)
        {
            article.Id = idGen.NextId();
            foreach (var tag in article.Tags)
                tag.ArticleId = article.Id;
        }

        db.Articles.AddRange(list);
        await db.SaveChangesAsync();
        return list;
    }
}

