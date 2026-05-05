using System.ComponentModel.DataAnnotations;

namespace BlogBank.Api.Models;

/// <summary>分配或撤销用户角色的请求体。</summary>
/// <param name="UserId">用户的雪花 ID（字符串形式，避免 JS 精度丢失）。</param>
/// <param name="RoleId">角色的自增 ID。</param>
public record UserRoleRequest(
    [Required] string UserId,
    [Required] int    RoleId
);
