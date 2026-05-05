namespace BlogBank.Core.Entities;

/// <summary>
/// 用户菜单关联实体，表示用户与菜单的多对多关系，用于个性化菜单权限分配。
/// </summary>
public class UserMenu
{
    /// <summary>
    /// 关联的用户 ID（联合主键之一）。
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 关联的菜单 ID（联合主键之一）。
    /// </summary>
    public long MenuId { get; set; }

    /// <summary>
    /// 授权时间（UTC）。
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 导航属性：关联的用户。
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// 导航属性：关联的菜单。
    /// </summary>
    public Menu Menu { get; set; } = null!;
}
