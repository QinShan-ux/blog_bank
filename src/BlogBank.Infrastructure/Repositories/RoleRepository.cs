using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogBank.Infrastructure.Repositories;

/// <summary>
/// 角色仓储实现，基于 EF Core 对 <see cref="Role"/> 进行持久化操作。
/// </summary>
public class RoleRepository(AppDbContext db) : IRoleRepository
{
    /// <summary>
    /// 获取所有角色，按创建时间倒序排列。
    /// </summary>
    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await db.Roles
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// 按 ID 查询单个角色。
    /// </summary>
    public async Task<Role?> GetByIdAsync(int id)
    {
        return await db.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    /// <summary>
    /// 新增角色，ID 由数据库自增生成，自动设置创建时间为当前 UTC。
    /// </summary>
    public async Task<Role> CreateAsync(Role role)
    {
        role.CreatedAt = DateTime.UtcNow;
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        return role;
    }

    /// <summary>
    /// 全量更新角色的编码、名称和描述字段。
    /// </summary>
    public async Task<Role?> UpdateAsync(int id, Role updated)
    {
        var existing = await db.Roles.FindAsync(id);
        if (existing is null) return null;

        existing.Code        = updated.Code;
        existing.Name        = updated.Name;
        existing.Description = updated.Description;

        await db.SaveChangesAsync();
        return existing;
    }

    /// <summary>
    /// 删除指定角色，用户角色关联由数据库级联约束自动清除。
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var role = await db.Roles.FindAsync(id);
        if (role is null) return false;

        db.Roles.Remove(role);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 检查角色编码是否已被其他角色占用；<paramref name="excludeId"/> 用于排除当前角色自身。
    /// </summary>
    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        return await db.Roles
            .AnyAsync(r => r.Code == code && (excludeId == null || r.Id != excludeId));
    }
}
