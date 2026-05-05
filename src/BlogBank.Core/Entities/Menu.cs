using BlogBank.Core.Enums;

namespace BlogBank.Core.Entities;

/// <summary>
/// 系统菜单实体，支持无限层级的父子结构。
/// </summary>
public class Menu: BaseEntity
{

    /// <summary>
    /// 父菜单 ID；根菜单设为 0。
    /// </summary>
    public long Pid { get; set; }

    /// <summary>
    /// 菜单类型：1 目录、2 菜单、3 按钮。
    /// </summary>
    public MenuTypeEnum Type { get; set; }

    /// <summary>
    /// 路由名称，对应前端 Vue Router 的 name 字段，最长 64 字符。
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 路由地址，对应前端 Vue Router 的 path 字段，最长 128 字符。
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// 组件文件路径，相对于 views 目录，最长 128 字符。
    /// </summary>
    public string? Component { get; set; }

    /// <summary>
    /// 重定向地址，目录类型菜单使用，最长 128 字符。
    /// </summary>
    public string? Redirect { get; set; }

    /// <summary>
    /// 权限标识，格式如 "sys:user:add"，用于后端鉴权，最长 128 字符。
    /// </summary>
    public string? Permission { get; set; }

    /// <summary>
    /// 菜单显示名称，最长 64 字符。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 菜单图标，格式如 "ele-Menu"，最长 128 字符。
    /// </summary>
    public string? Icon { get; set; } = "ele-Menu";

    /// <summary>
    /// 是否以内嵌 iframe 方式打开。
    /// </summary>
    public bool IsIframe { get; set; }

    /// <summary>
    /// 外链 URL，IsIframe 为 true 或需要外部跳转时使用，最长 256 字符。
    /// </summary>
    public string? OutLink { get; set; }

    /// <summary>
    /// 是否在侧边栏中隐藏该菜单。
    /// </summary>
    public bool IsHide { get; set; }

    /// <summary>
    /// 是否开启页面缓存（keep-alive），默认开启。
    /// </summary>
    public bool IsKeepAlive { get; set; } = true;

    /// <summary>
    /// 是否固定在标签栏（tab），固定后不可关闭。
    /// </summary>
    public bool IsAffix { get; set; }

    /// <summary>
    /// 排序号，数值越小越靠前，默认 100。
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 菜单状态：0 禁用、1 启用，默认启用。
    /// </summary>
    public MenuStatusEnum Status { get; set; } = MenuStatusEnum.Enabled;

    /// <summary>
    /// 备注，最长 256 字符。
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 用户菜单关联列表。
    /// </summary>
    public List<UserMenu> UserMenus { get; set; } = [];

    /// <summary>
    /// 子菜单列表，不映射到数据库，仅用于构建树形响应。
    /// </summary>
    public List<Menu> Children { get; set; } = [];
}
