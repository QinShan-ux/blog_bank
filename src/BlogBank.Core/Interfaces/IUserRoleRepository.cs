using BlogBank.Core.Entities;

namespace BlogBank.Core.Interfaces;

/// <summary>
/// 用户角色关联仓储接口，定义对 <see cref="UserRole"/> 的持久化操作。
/// </summary>
public interface IUserRoleRepository
{
    /// <summary>
    /// 查询指定用户拥有的所有角色关联（含角色详情）。
    /// </summary>
    /// <param name="userId">用户的雪花 ID。</param>
    /// <returns>该用户的角色关联集合。</returns>
    Task<IEnumerable<UserRole>> GetByUserIdAsync(long userId);

    /// <summary>
    /// 查询拥有指定角色的所有用户关联（含用户详情）。
    /// </summary>
    /// <param name="roleId">角色的自增 ID。</param>
    /// <returns>拥有该角色的用户关联集合。</returns>
    Task<IEnumerable<UserRole>> GetByRoleIdAsync(int roleId);

    /// <summary>
    /// 为用户分配角色。若关联已存在则直接返回现有记录，不重复插入。
    /// </summary>
    /// <param name="userId">用户的雪花 ID。</param>
    /// <param name="roleId">角色的自增 ID。</param>
    /// <returns>新创建或已存在的关联实体。</returns>
    Task<UserRole> AssignAsync(long userId, int roleId);

    /// <summary>
    /// 撤销用户的指定角色。
    /// </summary>
    /// <param name="userId">用户的雪花 ID。</param>
    /// <param name="roleId">角色的自增 ID。</param>
    /// <returns>撤销成功返回 <see langword="true"/>；关联不存在返回 <see langword="false"/>。</returns>
    Task<bool> RevokeAsync(long userId, int roleId);

    /// <summary>
    /// 检查指定的用户角色关联是否存在。
    /// </summary>
    /// <param name="userId">用户的雪花 ID。</param>
    /// <param name="roleId">角色的自增 ID。</param>
    /// <returns>存在返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    Task<bool> ExistsAsync(long userId, int roleId);
}
