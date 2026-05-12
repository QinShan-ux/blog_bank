using System.Text.Json;
using AutoMapper;
using BlogBank.Api.Dtos;
using BlogBank.Api.Models;
using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogBank.Api.Controllers;

/// <summary>
/// 用户管理控制器，提供用户的增删改查 REST 接口。
/// </summary>
[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController(IUserRepository repo, ICacheService cache,IMapper mapper,ILogger<UsersController> logger) : ControllerBase
{
    /// <summary>获取所有用户列表，按创建时间倒序排列。</summary>
    // GET /api/users
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var cached = await cache.GetAsync("users:all");
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var users = await repo.GetAllAsync();
        var data = users.Select(ToResponse).ToList();
        await cache.SetAsync("users:all", JsonSerializer.Serialize(data), "Users");
        return Ok(data);
    }

    /// <summary>按 ID 获取单个用户（含角色信息）。</summary>
    // GET /api/users/{id}
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        // var cached = await cache.GetAsync($"users:{id}");
        // if (cached != null)
        //     return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var user = await repo.GetByIdAsync(id);
        var res = mapper.Map<UserTestDto>(user);
        logger.LogInformation("用户信息 {@user}",res);
        logger.LogInformation(res.youxiang);
        if (user is null) return NotFound();

        // var data = ToResponse(user);
        await cache.SetAsync($"users:{id}", JsonSerializer.Serialize(res), "Users");
        return Ok(res);
    }

    /// <summary>
    /// 新增用户，ID 由服务端雪花算法自动生成。
    /// 若用户名或邮箱已被占用则返回 409 Conflict。
    /// </summary>
    // POST /api/users
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] UserRequest req)
    {
        if (await repo.UsernameExistsAsync(req.Username))
            return Conflict(new { message = $"用户名 '{req.Username}' 已被使用。" });

        if (await repo.EmailExistsAsync(req.Email))
            return Conflict(new { message = $"邮箱 '{req.Email}' 已被使用。" });

        var created = await repo.CreateAsync(ToEntity(req));
        await cache.RemoveAsync("users:all");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
    }

    /// <summary>
    /// 全量更新指定 ID 的用户信息；Password 为空时不修改密码。
    /// 若用户名或邮箱与其他用户冲突则返回 409 Conflict。
    /// </summary>
    // PUT /api/users/{id}
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(long id, [FromBody] UserRequest req)
    {
        if (await repo.UsernameExistsAsync(req.Username, excludeId: id))
            return Conflict(new { message = $"用户名 '{req.Username}' 已被使用。" });

        if (await repo.EmailExistsAsync(req.Email, excludeId: id))
            return Conflict(new { message = $"邮箱 '{req.Email}' 已被使用。" });

        var updated = await repo.UpdateAsync(id, ToEntity(req));
        if (updated is null) return NotFound();
        await cache.RemoveAsync("users:all", $"users:{id}");
        return Ok(ToResponse(updated));
    }

    /// <summary>删除指定 ID 的用户及其所有角色关联。</summary>
    // DELETE /api/users/{id}
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await repo.DeleteAsync(id);
        if (!deleted) return NotFound();
        await cache.RemoveAsync("users:all", $"users:{id}");
        return NoContent();
    }

    /// <summary>将请求体映射为用户实体；密码字段直接存储（生产环境应在此处进行哈希处理）。</summary>
    private static User ToEntity(UserRequest req) => new()
    {
        Username     = req.Username,
        Nickname     = req.Nickname,
        Email        = req.Email,
        PasswordHash = string.IsNullOrEmpty(req.Password)
            ? string.Empty
            : BCrypt.Net.BCrypt.HashPassword(req.Password),
        Avatar       = req.Avatar,
        IsEnabled    = req.IsEnabled
    };

    /// <summary>将用户实体映射为 API 响应对象；id 序列化为字符串，避免 JavaScript 53 位精度丢失。</summary>
    private static object ToResponse(User u) => new
    {
        id        = u.Id.ToString(),
        username  = u.Username,
        nickname  = u.Nickname,
        email     = u.Email,
        avatar    = u.Avatar,
        isEnabled = u.IsEnabled,
        createdAt = u.CreatedAt,
        updatedAt = u.UpdatedAt,
        roles     = u.UserRoles.Select(ur => new
        {
            id   = ur.Role.Id,
            code = ur.Role.Code,
            name = ur.Role.Name
        })
    };
}
