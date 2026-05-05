using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogBank.Infrastructure.Repositories;

/// <summary>
/// 菜单仓储实现，基于 EF Core 对 <see cref="Menu"/> 进行持久化操作。
/// </summary>
public class MenuRepository(AppDbContext db, ISnowflakeIdGenerator idGen) : IMenuRepository
{
    /// <summary>
    /// 获取所有菜单扁平列表，按排序号升序排列。
    /// </summary>
    public async Task<IEnumerable<Menu>> GetAllAsync()
    {
        return await db.Menus
            .OrderBy(m => m.OrderNo)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// 按 ID 查询单个菜单。
    /// </summary>
    public async Task<Menu?> GetByIdAsync(long id)
    {
        return await db.Menus
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    /// <summary>
    /// 查询指定父节点的直接子菜单，按排序号升序排列。
    /// </summary>
    public async Task<IEnumerable<Menu>> GetChildrenAsync(long pid)
    {
        return await db.Menus
            .Where(m => m.Pid == pid)
            .OrderBy(m => m.OrderNo)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// 新增菜单，由雪花算法生成 ID。
    /// </summary>
    public async Task<Menu> CreateAsync(Menu menu)
    {
        menu.Id = idGen.NextId();
        db.Menus.Add(menu);
        await db.SaveChangesAsync();
        return menu;
    }

    /// <summary>
    /// 全量更新菜单所有可编辑字段。
    /// </summary>
    public async Task<Menu?> UpdateAsync(long id, Menu updated)
    {
        var existing = await db.Menus.FindAsync(id);
        if (existing is null) return null;

        existing.Pid         = updated.Pid;
        existing.Type        = updated.Type;
        existing.Name        = updated.Name;
        existing.Path        = updated.Path;
        existing.Component   = updated.Component;
        existing.Redirect    = updated.Redirect;
        existing.Permission  = updated.Permission;
        existing.Title       = updated.Title;
        existing.Icon        = updated.Icon;
        existing.IsIframe    = updated.IsIframe;
        existing.OutLink     = updated.OutLink;
        existing.IsHide      = updated.IsHide;
        existing.IsKeepAlive = updated.IsKeepAlive;
        existing.IsAffix     = updated.IsAffix;
        existing.OrderNo     = updated.OrderNo;
        existing.Status      = updated.Status;
        existing.Remark      = updated.Remark;

        await db.SaveChangesAsync();
        return existing;
    }

    /// <summary>
    /// 删除指定菜单；建议先检查是否有子菜单再调用此方法。
    /// </summary>
    public async Task<bool> DeleteAsync(long id)
    {
        var menu = await db.Menus.FindAsync(id);
        if (menu is null) return false;

        db.Menus.Remove(menu);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 检查指定菜单是否存在直接子菜单。
    /// </summary>
    public async Task<bool> HasChildrenAsync(long id)
    {
        return await db.Menus.AnyAsync(m => m.Pid == id);
    }
}
