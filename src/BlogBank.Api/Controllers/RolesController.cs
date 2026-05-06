using System.Text.Json;
using AutoMapper;
using BlogBank.Api.Filters;
using BlogBank.Api.Models;
using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogBank.Api.Controllers;

/// <summary>
/// 角色管理控制器，提供角色的增删改查 REST 接口。
/// </summary>
[Authorize]
[ApiController]
[Route("api/roles")]
public class RolesController(IRoleRepository repo, ICacheService cache,IMapper mapper) : ControllerBase
{
    /// <summary>获取所有角色列表，按创建时间倒序排列。</summary>
    // GET /api/roles
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var cached = await cache.GetAsync("roles:all");
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var roles = await repo.GetAllAsync();
        var data = roles.Select(ToResponse).ToList();
        await cache.SetAsync("roles:all", JsonSerializer.Serialize(data), "Roles");
        return Ok(data);
    }

    /// <summary>按 ID 获取单个角色。</summary>
    // GET /api/roles/{id}
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var cached = await cache.GetAsync($"roles:{id}");
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var role = await repo.GetByIdAsync(id);
        var res = mapper.Map<RoleDto>(role);
        if (role is null) return NotFound();

        // var data = ToResponse(role);
        // await cache.SetAsync($"roles:{id}", JsonSerializer.Serialize(data), "Roles");
        return Ok(res);
    }

    /// <summary>
    /// 新增角色，ID 由数据库自增生成。
    /// 若角色编码已被占用则返回 409 Conflict。
    /// </summary>
    // POST /api/roles
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Audit]
    public async Task<IActionResult> Create([FromBody] RoleRequest req)
    {
        if (await repo.CodeExistsAsync(req.Code))
            return Conflict(new { message = $"角色编码 '{req.Code}' 已被使用。" });

        var created = await repo.CreateAsync(ToEntity(req));
        await cache.RemoveAsync("roles:all");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
    }

    /// <summary>
    /// 全量更新指定 ID 的角色信息。
    /// 若角色编码与其他角色冲突则返回 409 Conflict。
    /// </summary>
    // PUT /api/roles/{id}
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] RoleRequest req)
    {
        if (await repo.CodeExistsAsync(req.Code, excludeId: id))
            return Conflict(new { message = $"角色编码 '{req.Code}' 已被使用。" });

        var updated = await repo.UpdateAsync(id, ToEntity(req));
        if (updated is null) return NotFound();
        await cache.RemoveAsync("roles:all", $"roles:{id}");
        return Ok(ToResponse(updated));
    }

    /// <summary>删除指定 ID 的角色及其所有用户关联。</summary>
    // DELETE /api/roles/{id}
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await repo.DeleteAsync(id);
        if (!deleted) return NotFound();
        await cache.RemoveAsync("roles:all", $"roles:{id}");
        return NoContent();
    }

    /// <summary>将请求体映射为角色实体。</summary>
    private static Role ToEntity(RoleRequest req) => new()
    {
        Code        = req.Code,
        Name        = req.Name,
        Description = req.Description,
        CreatedAt = DateTime.Now,
        UpdatedAt = DateTime.Now,
        CreatedBy = "--",
        UpdatedBy = "--",
    };

    /// <summary>将角色实体映射为 API 响应对象。</summary>
    private static object ToResponse(Role r) => new
    {
        id          = r.Id,
        code        = r.Code,
        name        = r.Name,
        description = r.Description,
        createdAt   = r.CreatedAt,
        
    };
}
