using System.ComponentModel.DataAnnotations;
using BlogBank.Core.Enums;

namespace BlogBank.Api.Models;

/// <summary>新增或更新菜单的请求体。</summary>
/// <param name="Pid">父菜单 ID，根菜单传 0。</param>
/// <param name="Type">菜单类型：1 目录、2 菜单、3 按钮。</param>
/// <param name="Title">菜单显示名称，最长 64 字符。</param>
/// <param name="Name">路由名称，最长 64 字符，目录类型可为空。</param>
/// <param name="Path">路由地址，最长 128 字符，按钮类型可为空。</param>
/// <param name="Component">组件文件路径，最长 128 字符，仅菜单类型需要。</param>
/// <param name="Redirect">重定向地址，最长 128 字符，目录类型使用。</param>
/// <param name="Permission">权限标识，格式如 "sys:user:add"，最长 128 字符。</param>
/// <param name="Icon">图标名称，格式如 "ele-Menu"，最长 128 字符。</param>
/// <param name="IsIframe">是否以内嵌 iframe 方式打开。</param>
/// <param name="OutLink">外链 URL，最长 256 字符。</param>
/// <param name="IsHide">是否在侧边栏中隐藏。</param>
/// <param name="IsKeepAlive">是否开启页面缓存，默认 true。</param>
/// <param name="IsAffix">是否固定在标签栏，默认 false。</param>
/// <param name="OrderNo">排序号，数值越小越靠前，默认 100。</param>
/// <param name="Status">菜单状态：0 禁用、1 启用，默认启用。</param>
/// <param name="Remark">备注，最长 256 字符。</param>
public record MenuRequest(
                                    long            Pid,
    [Required]                      MenuTypeEnum    Type,
    [Required][MaxLength(64)]       string          Title,
    [MaxLength(64)]                 string?         Name        = null,
    [MaxLength(128)]                string?         Path        = null,
    [MaxLength(128)]                string?         Component   = null,
    [MaxLength(128)]                string?         Redirect    = null,
    [MaxLength(128)]                string?         Permission  = null,
    [MaxLength(128)]                string?         Icon        = "ele-Menu",
                                    bool            IsIframe    = false,
    [MaxLength(256)]                string?         OutLink     = null,
                                    bool            IsHide      = false,
                                    bool            IsKeepAlive = true,
                                    bool            IsAffix     = false,
                                    int             OrderNo     = 100,
                                    MenuStatusEnum  Status      = MenuStatusEnum.Enabled,
    [MaxLength(256)]                string?         Remark      = null
);
