using BlogBank.Core.Entities;

namespace BlogBank.Core.Interfaces;

/// <summary>
/// 菜单仓储接口，定义对 <see cref="Menu"/> 的持久化操作。
/// </summary>
public interface IMenuRepository
{
    /// <summary>
    /// 获取所有菜单（扁平列表），按排序号升序排列。
    /// </summary>
    /// <returns>菜单集合。</returns>
    Task<IEnumerable<Menu>> GetAllAsync();

    /// <summary>
    /// 按 ID 查询单个菜单。
    /// </summary>
    /// <param name="id">菜单的雪花 ID。</param>
    /// <returns>匹配的菜单；若不存在则返回 <see langword="null"/>。</returns>
    Task<Menu?> GetByIdAsync(long id);

    /// <summary>
    /// 查询指定父节点的直接子菜单，按排序号升序排列。
    /// </summary>
    /// <param name="pid">父菜单 ID；传 0 表示查询根菜单。</param>
    /// <returns>子菜单集合。</returns>
    Task<IEnumerable<Menu>> GetChildrenAsync(long pid);

    /// <summary>
    /// 新增菜单，ID 由雪花算法在仓储内部生成。
    /// </summary>
    /// <param name="menu">待新增的菜单实体（无需设置 <see cref="Menu.Id"/>）。</param>
    /// <returns>已赋予 ID 的菜单实体。</returns>
    Task<Menu> CreateAsync(Menu menu);

    /// <summary>
    /// 全量更新指定 ID 的菜单信息。
    /// </summary>
    /// <param name="id">目标菜单的雪花 ID。</param>
    /// <param name="menu">携带最新数据的菜单实体。</param>
    /// <returns>更新后的菜单实体；若 ID 不存在则返回 <see langword="null"/>。</returns>
    Task<Menu?> UpdateAsync(long id, Menu menu);

    /// <summary>
    /// 删除指定 ID 的菜单。
    /// </summary>
    /// <param name="id">目标菜单的雪花 ID。</param>
    /// <returns>删除成功返回 <see langword="true"/>；ID 不存在返回 <see langword="false"/>。</returns>
    Task<bool> DeleteAsync(long id);

    /// <summary>
    /// 检查指定菜单是否存在直接子菜单。
    /// </summary>
    /// <param name="id">菜单的雪花 ID。</param>
    /// <returns>有子菜单返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    Task<bool> HasChildrenAsync(long id);
}
