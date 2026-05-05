namespace BlogBank.Core.Entities;

/// <summary>
/// 用户角色关联实体，表示用户与角色的多对多关系。
/// </summary>
public class UserRole
{
    /// <summary>
    /// 关联的用户 ID（联合主键之一）。
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 关联的角色 ID（联合主键之一）。
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// 授权时间（UTC）。
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 导航属性：关联的用户。
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// 导航属性：关联的角色。
    /// </summary>
    public Role Role { get; set; } = null!;
}
