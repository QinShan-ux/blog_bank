using BlogBank.Core.Entities;

namespace BlogBank.Core.Interfaces;

/// <summary>
/// 用户仓储接口，定义对 <see cref="User"/> 的持久化操作。
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// 获取所有用户，按创建时间倒序排列。
    /// </summary>
    /// <returns>用户集合。</returns>
    Task<IEnumerable<User>> GetAllAsync();

    /// <summary>
    /// 按 ID 查询单个用户（含角色关联）。
    /// </summary>
    /// <param name="id">用户的雪花 ID。</param>
    /// <returns>匹配的用户；若不存在则返回 <see langword="null"/>。</returns>
    Task<User?> GetByIdAsync(long id);

    /// <summary>
    /// 新增用户，ID 由雪花算法在仓储内部生成。
    /// </summary>
    /// <param name="user">待新增的用户实体（无需设置 <see cref="User.Id"/>）。</param>
    /// <returns>已赋予 ID 的用户实体。</returns>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// 全量更新指定 ID 的用户信息。
    /// </summary>
    /// <param name="id">目标用户的雪花 ID。</param>
    /// <param name="user">携带最新数据的用户实体。</param>
    /// <returns>更新后的用户实体；若 ID 不存在则返回 <see langword="null"/>。</returns>
    Task<User?> UpdateAsync(long id, User user);

    /// <summary>
    /// 删除指定 ID 的用户及其所有角色关联。
    /// </summary>
    /// <param name="id">目标用户的雪花 ID。</param>
    /// <returns>删除成功返回 <see langword="true"/>；ID 不存在返回 <see langword="false"/>。</returns>
    Task<bool> DeleteAsync(long id);

    /// <summary>
    /// 检查用户名是否已被使用。
    /// </summary>
    /// <param name="username">待检查的用户名。</param>
    /// <param name="excludeId">排除的用户 ID（更新时使用）。</param>
    /// <returns>已存在返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    Task<bool> UsernameExistsAsync(string username, long? excludeId = null);

    /// <summary>
    /// 检查邮箱是否已被使用。
    /// </summary>
    /// <param name="email">待检查的邮箱地址。</param>
    /// <param name="excludeId">排除的用户 ID（更新时使用）。</param>
    /// <returns>已存在返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    Task<bool> EmailExistsAsync(string email, long? excludeId = null);

    /// <summary>
    /// 按用户名查询用户。
    /// </summary>
    Task<User?> GetByUsernameAsync(string username);
}
