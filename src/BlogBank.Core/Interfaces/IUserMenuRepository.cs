using BlogBank.Core.Entities;

namespace BlogBank.Core.Interfaces;

/// <summary>
/// 用户菜单关联仓储接口，定义对 <see cref="UserMenu"/> 的持久化操作。
/// </summary>
public interface IUserMenuRepository
{
    /// <summary>
    /// 查询指定用户拥有的所有菜单关联（含菜单详情），按排序号升序排列。
    /// </summary>
    /// <param name="userId">用户的雪花 ID。</param>
    /// <returns>该用户的菜单关联集合。</returns>
    Task<IEnumerable<UserMenu>> GetByUserIdAsync(long userId);

    /// <summary>
    /// 为用户分配菜单权限。若关联已存在则直接返回现有记录，不重复插入。
    /// </summary>
    /// <param name="userId">用户的雪花 ID。</param>
    /// <param name="menuId">菜单的雪花 ID。</param>
    /// <returns>新创建或已存在的关联实体。</returns>
    Task<UserMenu> AssignAsync(long userId, long menuId);

    /// <summary>
    /// 批量为用户分配菜单权限，自动跳过已存在的关联。
    /// </summary>
    /// <param name="userId">用户的雪花 ID。</param>
    /// <param name="menuIds">待分配的菜单 ID 集合。</param>
    /// <returns>本次实际新增的关联数量。</returns>
    Task<int> BatchAssignAsync(long userId, IEnumerable<long> menuIds);

    /// <summary>
    /// 撤销用户的指定菜单权限。
    /// </summary>
    /// <param name="userId">用户的雪花 ID。</param>
    /// <param name="menuId">菜单的雪花 ID。</param>
    /// <returns>撤销成功返回 <see langword="true"/>；关联不存在返回 <see langword="false"/>。</returns>
    Task<bool> RevokeAsync(long userId, long menuId);

    /// <summary>
    /// 重置用户的菜单权限：清除全部旧关联，再批量写入新关联。
    /// </summary>
    /// <param name="userId">用户的雪花 ID。</param>
    /// <param name="menuIds">新的菜单 ID 集合。</param>
    Task ResetAsync(long userId, IEnumerable<long> menuIds);

    /// <summary>
    /// 检查指定的用户菜单关联是否存在。
    /// </summary>
    /// <param name="userId">用户的雪花 ID。</param>
    /// <param name="menuId">菜单的雪花 ID。</param>
    /// <returns>存在返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    Task<bool> ExistsAsync(long userId, long menuId);
}
