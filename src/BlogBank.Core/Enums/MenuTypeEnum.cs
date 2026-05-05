namespace BlogBank.Core.Enums;

/// <summary>
/// 菜单类型枚举。
/// </summary>
public enum MenuTypeEnum
{
    /// <summary>目录，用于分组，不对应具体页面。</summary>
    Directory = 1,

    /// <summary>菜单，对应一个前端路由页面。</summary>
    Menu = 2,

    /// <summary>按钮，对应页面内的操作权限点。</summary>
    Button = 3
}
