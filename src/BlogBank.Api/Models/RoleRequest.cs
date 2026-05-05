using System.ComponentModel.DataAnnotations;

namespace BlogBank.Api.Models;

/// <summary>新增或更新角色的请求体。</summary>
/// <param name="Code">角色编码，程序内部使用，全局唯一，例如 "admin"，最长 50 字符。</param>
/// <param name="Name">角色显示名称，例如"管理员"，最长 100 字符。</param>
/// <param name="Description">角色描述，说明该角色的权限范围，最长 500 字符。</param>
public record RoleRequest(
    [Required][MaxLength(50)]  string Code,
    [Required][MaxLength(100)] string Name,
    [MaxLength(500)]           string Description = ""
);
