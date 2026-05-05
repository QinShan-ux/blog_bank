using System.Text.Json;
using BlogBank.Api.Models;
using BlogBank.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogBank.Api.Controllers;

/// <summary>
/// 用户角色关联控制器，提供用户角色的分配、撤销与查询 REST 接口。
/// </summary>
[Authorize]
[ApiController]
[Route("api/user-roles")]
public class UserRolesController(IUserRoleRepository repo, ICacheService cache) : ControllerBase
{
    /// <summary>查询指定用户拥有的所有角色。</summary>
    // GET /api/user-roles/user/{userId}
    [HttpGet("user/{userId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUser(long userId)
    {
        var cached = await cache.GetAsync($"user-roles:user:{userId}");
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var items = await repo.GetByUserIdAsync(userId);
        var data = items.Select(ur => new
        {
            userId     = ur.UserId.ToString(),
            roleId     = ur.RoleId,
            roleCode   = ur.Role.Code,
            roleName   = ur.Role.Name,
            assignedAt = ur.AssignedAt
        }).ToList();
        await cache.SetAsync($"user-roles:user:{userId}", JsonSerializer.Serialize(data), "UserRoles");
        return Ok(data);
    }

    /// <summary>查询拥有指定角色的所有用户。</summary>
    // GET /api/user-roles/role/{roleId}
    [HttpGet("role/{roleId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRole(int roleId)
    {
        var cached = await cache.GetAsync($"user-roles:role:{roleId}");
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var items = await repo.GetByRoleIdAsync(roleId);
        var data = items.Select(ur => new
        {
            userId     = ur.UserId.ToString(),
            username   = ur.User.Username,
            nickname   = ur.User.Nickname,
            roleId     = ur.RoleId,
            assignedAt = ur.AssignedAt
        }).ToList();
        await cache.SetAsync($"user-roles:role:{roleId}", JsonSerializer.Serialize(data), "UserRoles");
        return Ok(data);
    }

    /// <summary>
    /// 为用户分配角色；若关联已存在则直接返回，保证幂等性。
    /// UserId 以字符串形式传入，避免 JavaScript 53 位精度丢失。
    /// </summary>
    // POST /api/user-roles
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Assign([FromBody] UserRoleRequest req)
    {
        if (!long.TryParse(req.UserId, out var userId))
            return BadRequest(new { message = "UserId 格式无效。" });

        var userRole = await repo.AssignAsync(userId, req.RoleId);
        await cache.RemoveAsync($"user-roles:user:{userId}", $"user-roles:role:{req.RoleId}", $"users:{userId}");
        return Ok(new
        {
            userId     = userRole.UserId.ToString(),
            roleId     = userRole.RoleId,
            assignedAt = userRole.AssignedAt
        });
    }

    /// <summary>
    /// 撤销用户的指定角色；关联不存在时返回 404 Not Found。
    /// UserId 以字符串形式传入，避免 JavaScript 53 位精度丢失。
    /// </summary>
    // DELETE /api/user-roles
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke([FromBody] UserRoleRequest req)
    {
        if (!long.TryParse(req.UserId, out var userId))
            return BadRequest(new { message = "UserId 格式无效。" });

        var revoked = await repo.RevokeAsync(userId, req.RoleId);
        if (!revoked) return NotFound();
        await cache.RemoveAsync($"user-roles:user:{userId}", $"user-roles:role:{req.RoleId}", $"users:{userId}");
        return NoContent();
    }
}
