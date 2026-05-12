namespace BlogBank.Core.Entities;

/// <summary>
/// 用户实体。
/// </summary>
public class User: BaseEntity
{

    /// <summary>
    /// 用户名，用于登录，全局唯一。
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 显示昵称。
    /// </summary>
    public string Nickname { get; set; } = string.Empty;

    
    /// <summary>
    /// 电话。
    /// </summary>
    public string Phone { get; set; } = string.Empty;
    
    /// <summary>
    /// 邮箱地址，全局唯一。
    /// </summary>
    
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 经过哈希处理的密码。
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 头像 URL。
    /// </summary>
    public string Avatar { get; set; } = string.Empty;

    /// <summary>
    /// 账号是否启用。
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    
    /// <summary>
    /// token版本号
    /// </summary>
    public int TokenVersion { get; set; }
    

    /// <summary>
    /// 用户拥有的角色关联列表。
    /// </summary>
    public List<UserRole> UserRoles { get; set; } = [];

    /// <summary>
    /// 用户拥有的菜单关联列表。
    /// </summary>
    public List<UserMenu> UserMenus { get; set; } = [];
}
