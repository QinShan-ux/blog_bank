namespace BlogBank.Core.Entities;

/// <summary>
/// 角色实体。
/// </summary>
public class Role: BaseEntity
{

    /// <summary>
    /// 角色编码，程序内部使用，全局唯一，例如 "admin"、"editor"。
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 角色显示名称，例如"管理员"、"编辑者"。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色描述，说明该角色的权限范围。
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间（UTC）。
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 拥有此角色的用户关联列表。
    /// </summary>
    public List<UserRole> UserRoles { get; set; } = [];
}
