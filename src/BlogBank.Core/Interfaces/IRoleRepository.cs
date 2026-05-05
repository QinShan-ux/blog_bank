using BlogBank.Core.Entities;

namespace BlogBank.Core.Interfaces;

/// <summary>
/// 角色仓储接口，定义对 <see cref="Role"/> 的持久化操作。
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// 获取所有角色，按创建时间倒序排列。
    /// </summary>
    /// <returns>角色集合。</returns>
    Task<IEnumerable<Role>> GetAllAsync();

    /// <summary>
    /// 按 ID 查询单个角色。
    /// </summary>
    /// <param name="id">角色的自增 ID。</param>
    /// <returns>匹配的角色；若不存在则返回 <see langword="null"/>。</returns>
    Task<Role?> GetByIdAsync(int id);

    /// <summary>
    /// 新增角色，ID 由数据库自增生成。
    /// </summary>
    /// <param name="role">待新增的角色实体（无需设置 <see cref="Role.Id"/>）。</param>
    /// <returns>已赋予 ID 的角色实体。</returns>
    Task<Role> CreateAsync(Role role);

    /// <summary>
    /// 全量更新指定 ID 的角色信息。
    /// </summary>
    /// <param name="id">目标角色的自增 ID。</param>
    /// <param name="role">携带最新数据的角色实体。</param>
    /// <returns>更新后的角色实体；若 ID 不存在则返回 <see langword="null"/>。</returns>
    Task<Role?> UpdateAsync(int id, Role role);

    /// <summary>
    /// 删除指定 ID 的角色及其所有用户关联。
    /// </summary>
    /// <param name="id">目标角色的自增 ID。</param>
    /// <returns>删除成功返回 <see langword="true"/>；ID 不存在返回 <see langword="false"/>。</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// 检查角色编码是否已被使用。
    /// </summary>
    /// <param name="code">待检查的角色编码。</param>
    /// <param name="excludeId">排除的角色 ID（更新时使用）。</param>
    /// <returns>已存在返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);
}
