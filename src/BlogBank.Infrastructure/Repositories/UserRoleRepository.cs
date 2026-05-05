using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogBank.Infrastructure.Repositories;

/// <summary>
/// 用户角色关联仓储实现，基于 EF Core 对 <see cref="UserRole"/> 进行持久化操作。
/// </summary>
public class UserRoleRepository(AppDbContext db) : IUserRoleRepository
{
    /// <summary>
    /// 查询指定用户的所有角色关联，预加载角色详情。
    /// </summary>
    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(long userId)
    {
        return await db.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// 查询拥有指定角色的所有用户关联，预加载用户详情。
    /// </summary>
    public async Task<IEnumerable<UserRole>> GetByRoleIdAsync(int roleId)
    {
        return await db.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.RoleId == roleId)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// 为用户分配角色；若关联已存在则直接返回现有记录，保证幂等性。
    /// </summary>
    public async Task<UserRole> AssignAsync(long userId, int roleId)
    {
        var existing = await db.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        // 已存在则直接返回，避免重复插入
        if (existing is not null) return existing;

        var userRole = new UserRole
        {
            UserId     = userId,
            RoleId     = roleId,
            AssignedAt = DateTime.UtcNow
        };

        db.UserRoles.Add(userRole);
        await db.SaveChangesAsync();
        return userRole;
    }

    /// <summary>
    /// 撤销用户的指定角色关联。
    /// </summary>
    public async Task<bool> RevokeAsync(long userId, int roleId)
    {
        var userRole = await db.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole is null) return false;

        db.UserRoles.Remove(userRole);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 检查指定的用户角色关联是否存在。
    /// </summary>
    public async Task<bool> ExistsAsync(long userId, int roleId)
    {
        return await db.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }
}
