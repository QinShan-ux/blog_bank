using System.ComponentModel.DataAnnotations;

namespace BlogBank.Api.Models;

/// <summary>新增或更新用户的请求体。</summary>
/// <param name="Username">用户名，用于登录，全局唯一，最长 50 字符。</param>
/// <param name="Nickname">显示昵称，最长 100 字符。</param>
/// <param name="Email">邮箱地址，全局唯一，最长 200 字符。</param>
/// <param name="Password">明文密码；更新时留空表示不修改密码。</param>
/// <param name="Avatar">头像 URL，最长 500 字符。</param>
/// <param name="IsEnabled">是否启用账号，默认为 <see langword="true"/>。</param>
public record UserRequest(
    [Required][MaxLength(50)]   string Username,
    [Required][MaxLength(100)]  string Nickname,
    [Required][EmailAddress][MaxLength(200)] string Email,
                                string? Password,
    [MaxLength(500)]            string Avatar = "",
                                bool IsEnabled = true
);
