using System.Text.Json;
using BlogBank.Api.Models;
using BlogBank.Core.Interfaces;
using BlogBank.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogBank.Api.Controllers;

/// <summary>
/// 用户菜单关联控制器，提供用户菜单权限的分配、撤销与查询 REST 接口。
/// </summary>
[Authorize]
[ApiController]
[Route("api/user-menus")]
public class UserMenusController(IUserMenuService service, ICacheService cache) : ControllerBase
{
    /// <summary>
    /// 查询指定用户拥有的所有菜单权限，按排序号升序排列。
    /// </summary>
    // GET /api/user-menus/user/{userId}
    [HttpGet("user/{userId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUser(long userId)
    {
        var cached = await cache.GetAsync($"user-menus:user:{userId}");
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var items = await service.GetByUserIdAsync(userId);
        var data = items.Select(um => new
        {
            userId     = um.UserId.ToString(),
            menuId     = um.MenuId.ToString(),
            menuTitle  = um.Menu.Title,
            menuType   = (int)um.Menu.Type,
            assignedAt = um.AssignedAt
        }).ToList();
        await cache.SetAsync($"user-menus:user:{userId}", JsonSerializer.Serialize(data), "UserMenus");
        return Ok(data);
    }

    /// <summary>为用户分配单个菜单权限；若关联已存在则幂等返回。</summary>
    // POST /api/user-menus
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Assign([FromBody] UserMenuRequest req)
    {
        if (!long.TryParse(req.UserId, out var userId))
            return BadRequest(new { message = "UserId 格式无效。" });

        var userMenu = await service.AssignAsync(userId, req.MenuId);
        await cache.RemoveAsync($"user-menus:user:{userId}");
        return Ok(new
        {
            userId     = userMenu.UserId.ToString(),
            menuId     = userMenu.MenuId.ToString(),
            assignedAt = userMenu.AssignedAt
        });
    }

    /// <summary>批量为用户分配菜单权限，自动跳过已存在的关联，返回本次实际新增数量。</summary>
    // POST /api/user-menus/batch
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BatchAssign([FromBody] UserMenuBatchRequest req)
    {
        if (!long.TryParse(req.UserId, out var userId))
            return BadRequest(new { message = "UserId 格式无效。" });

        var added = await service.BatchAssignAsync(userId, req.MenuIds);
        await cache.RemoveAsync($"user-menus:user:{userId}");
        return Ok(new { added });
    }

    /// <summary>重置用户的菜单权限：清除全部旧关联，再全量写入新关联。</summary>
    // PUT /api/user-menus/reset
    [HttpPut("reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reset([FromBody] UserMenuBatchRequest req)
    {
        if (!long.TryParse(req.UserId, out var userId))
            return BadRequest(new { message = "UserId 格式无效。" });

        await service.ResetAsync(userId, req.MenuIds);
        await cache.RemoveAsync($"user-menus:user:{userId}");
        return NoContent();
    }

    /// <summary>撤销用户的指定菜单权限；关联不存在时返回 404 Not Found。</summary>
    // DELETE /api/user-menus
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke([FromBody] UserMenuRequest req)
    {
        if (!long.TryParse(req.UserId, out var userId))
            return BadRequest(new { message = "UserId 格式无效。" });

        var revoked = await service.RevokeAsync(userId, req.MenuId);
        if (!revoked) return NotFound();
        await cache.RemoveAsync($"user-menus:user:{userId}");
        return NoContent();
    }
}
