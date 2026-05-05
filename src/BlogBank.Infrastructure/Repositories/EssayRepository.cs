using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogBank.Infrastructure.Repositories;

/// <summary>
/// 随笔仓储实现，基于 EF Core 对 <see cref="Essay"/> 进行持久化操作。
/// </summary>
public class EssayRepository(AppDbContext db, ISnowflakeIdGenerator idGen) : IEssayRepository
{
    /// <summary>
    /// 获取所有随笔及其标签，按发布日期倒序排列。
    /// </summary>
    public async Task<IEnumerable<Essay>> GetAllAsync()
    {
        return await db.Essays
            .Include(e => e.Tags)
            .OrderByDescending(e => e.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// 按 ID 查询单篇随笔（含标签）。
    /// </summary>
    public async Task<Essay?> GetByIdAsync(long id)
    {
        return await db.Essays
            .Include(e => e.Tags)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <summary>
    /// 新增随笔，由雪花算法生成 ID，并同步设置所有标签的外键。
    /// </summary>
    public async Task<Essay> CreateAsync(Essay essay)
    {
        essay.Id = idGen.NextId();
        foreach (var tag in essay.Tags)
            tag.EssayId = essay.Id;

        db.Essays.Add(essay);
        await db.SaveChangesAsync();
        return essay;
    }

    /// <summary>
    /// 全量更新随笔字段，先删除旧标签再批量插入新标签。
    /// </summary>
    public async Task<Essay?> UpdateAsync(long id, Essay updated)
    {
        // 使用AsNoTracking 不创建快照 不进行追踪
        var existing = await db.Essays
            .AsNoTracking()
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (existing is null) return null;

        // 把未追踪的实体"挂载"回 DbContext，设置状态为已修改
        db.Essays.Attach(existing).State = EntityState.Modified;
        existing.Title       = updated.Title;
        existing.Date        = updated.Date;
        existing.Mood        = updated.Mood;
        existing.MoodIcon    = updated.MoodIcon;
        existing.Weather     = updated.Weather;
        existing.WeatherIcon = updated.WeatherIcon;
        existing.Location    = updated.Location;
        existing.Excerpt     = updated.Excerpt;
        existing.Content     = updated.Content;
        existing.BgColor     = updated.BgColor;

        // 删除旧标签，重新写入新标签
        db.EssayTags.RemoveRange(existing.Tags);
        existing.Tags = updated.Tags
            .Select(t => new EssayTag { EssayId = id, Tag = t.Tag })
            .ToList();

        await db.SaveChangesAsync();
        return existing;
    }

    /// <summary>
    /// 删除指定随笔，级联删除由数据库约束处理。
    /// </summary>
    public async Task<bool> DeleteAsync(long id)
    {
        var essay = await db.Essays.FindAsync(id);
        if (essay is null) return false;

        db.Essays.Remove(essay);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 批量新增随笔，为每条记录生成雪花 ID，一次 SaveChanges 写入。
    /// </summary>
    public async Task<List<Essay>> CreateBatchAsync(IEnumerable<Essay> essays)
    {
        var list = essays.ToList();
        foreach (var essay in list)
        {
            essay.Id = idGen.NextId();
            foreach (var tag in essay.Tags)
                tag.EssayId = essay.Id;
        }

        db.Essays.AddRange(list);
        await db.SaveChangesAsync();
        return list;
    }
}
