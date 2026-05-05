using System.ComponentModel.DataAnnotations;

namespace BlogBank.Api.Models;

/// <summary>分配或撤销单个用户菜单权限的请求体。</summary>
/// <param name="UserId">用户的雪花 ID（字符串形式，避免 JS 精度丢失）。</param>
/// <param name="MenuId">菜单的雪花 ID。</param>
public record UserMenuRequest(
    [Required] string UserId,
    [Required] long   MenuId
);

/// <summary>批量分配用户菜单权限的请求体。</summary>
/// <param name="UserId">用户的雪花 ID（字符串形式，避免 JS 精度丢失）。</param>
/// <param name="MenuIds">待分配的菜单 ID 列表。</param>
public record UserMenuBatchRequest(
    [Required] string      UserId,
    [Required] List<long>  MenuIds
);
