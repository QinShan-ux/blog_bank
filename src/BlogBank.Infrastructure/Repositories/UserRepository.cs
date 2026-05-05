using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogBank.Infrastructure.Repositories;

/// <summary>
/// 用户仓储实现，基于 EF Core 对 <see cref="User"/> 进行持久化操作。
/// </summary>
public class UserRepository(AppDbContext db, ISnowflakeIdGenerator idGen) : IUserRepository
{
    /// <summary>
    /// 获取所有用户（不含角色关联），按创建时间倒序排列。
    /// </summary>
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await db.Users
            .OrderByDescending(u => u.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// 按 ID 查询单个用户，同时预加载用户角色及角色详情。
    /// </summary>
    public async Task<User?> GetByIdAsync(long id)
    {
        return await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// 新增用户，由雪花算法生成 ID，并自动设置创建/更新时间为当前 UTC。
    /// </summary>
    public async Task<User> CreateAsync(User user)
    {
        user.Id        = idGen.NextId();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// 全量更新用户基本信息；仅当请求中提供了非空密码时才更新密码哈希。
    /// </summary>
    public async Task<User?> UpdateAsync(long id, User updated)
    {
        var existing = await db.Users.FindAsync(id);
        if (existing is null) return null;

        existing.Username  = updated.Username;
        existing.Nickname  = updated.Nickname;
        existing.Email     = updated.Email;
        existing.Avatar    = updated.Avatar;
        existing.IsEnabled = updated.IsEnabled;
        existing.UpdatedAt = DateTime.UtcNow;

        // 密码字段非空时才更新，空字符串表示不修改密码
        if (!string.IsNullOrEmpty(updated.PasswordHash))
            existing.PasswordHash = updated.PasswordHash;

        await db.SaveChangesAsync();
        return existing;
    }

    /// <summary>
    /// 删除指定用户，用户角色关联由数据库级联约束自动清除。
    /// </summary>
    public async Task<bool> DeleteAsync(long id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return false;

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 检查用户名是否已被其他用户占用；<paramref name="excludeId"/> 用于排除当前用户自身。
    /// </summary>
    public async Task<bool> UsernameExistsAsync(string username, long? excludeId = null)
    {
        return await db.Users
            .AnyAsync(u => u.Username == username && (excludeId == null || u.Id != excludeId));
    }

    /// <summary>
    /// 检查邮箱是否已被其他用户占用；<paramref name="excludeId"/> 用于排除当前用户自身。
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email, long? excludeId = null)
    {
        return await db.Users
            .AnyAsync(u => u.Email == email && (excludeId == null || u.Id != excludeId));
    }

    /// <summary>
    /// 按用户名查询用户。
    /// </summary>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);
    }
}
