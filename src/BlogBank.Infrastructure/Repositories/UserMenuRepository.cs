using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogBank.Infrastructure.Repositories;

/// <summary>
/// 用户菜单关联仓储实现，基于 EF Core 对 <see cref="UserMenu"/> 进行持久化操作。
/// </summary>
public class UserMenuRepository(AppDbContext db) : IUserMenuRepository
{
    /// <summary>
    /// 查询指定用户拥有的所有菜单关联，预加载菜单详情，按排序号升序排列。
    /// </summary>
    public async Task<IEnumerable<UserMenu>> GetByUserIdAsync(long userId)
    {
        return await db.UserMenus
            .Include(um => um.Menu)
            .Where(um => um.UserId == userId)
            .OrderBy(um => um.Menu.OrderNo)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// 为用户分配菜单权限；若关联已存在则直接返回，保证幂等性。
    /// </summary>
    public async Task<UserMenu> AssignAsync(long userId, long menuId)
    {
        var existing = await db.UserMenus
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MenuId == menuId);

        // 已存在则直接返回，避免重复插入
        if (existing is not null) return existing;

        var userMenu = new UserMenu
        {
            UserId     = userId,
            MenuId     = menuId,
            AssignedAt = DateTime.UtcNow
        };

        db.UserMenus.Add(userMenu);
        await db.SaveChangesAsync();
        return userMenu;
    }

    /// <summary>
    /// 批量为用户分配菜单权限，自动跳过已存在的关联，一次事务提交。
    /// </summary>
    public async Task<int> BatchAssignAsync(long userId, IEnumerable<long> menuIds)
    {
        // 查出已有的菜单 ID，避免重复插入
        var existingIds = await db.UserMenus
            .Where(um => um.UserId == userId)
            .Select(um => um.MenuId)
            .ToListAsync();

        var toAdd = menuIds
            .Except(existingIds)
            .Select(menuId => new UserMenu
            {
                UserId     = userId,
                MenuId     = menuId,
                AssignedAt = DateTime.UtcNow
            })
            .ToList();

        if (toAdd.Count == 0) return 0;

        db.UserMenus.AddRange(toAdd);
        await db.SaveChangesAsync();
        return toAdd.Count;
    }

    /// <summary>
    /// 撤销用户的指定菜单权限。
    /// </summary>
    public async Task<bool> RevokeAsync(long userId, long menuId)
    {
        var userMenu = await db.UserMenus
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MenuId == menuId);

        if (userMenu is null) return false;

        db.UserMenus.Remove(userMenu);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 重置用户菜单权限：删除全部旧关联后，批量写入新关联，一次事务提交。
    /// </summary>
    public async Task ResetAsync(long userId, IEnumerable<long> menuIds)
    {
        // 删除旧关联
        var old = await db.UserMenus.Where(um => um.UserId == userId).ToListAsync();
        db.UserMenus.RemoveRange(old);

        // 写入新关联
        var newItems = menuIds.Select(menuId => new UserMenu
        {
            UserId     = userId,
            MenuId     = menuId,
            AssignedAt = DateTime.UtcNow
        });
        db.UserMenus.AddRange(newItems);

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// 检查指定的用户菜单关联是否存在。
    /// </summary>
    public async Task<bool> ExistsAsync(long userId, long menuId)
    {
        return await db.UserMenus
            .AnyAsync(um => um.UserId == userId && um.MenuId == menuId);
    }
}
