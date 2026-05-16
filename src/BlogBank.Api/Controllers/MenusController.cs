using System.Text.Json;
using BlogBank.Api.Models;
using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogBank.Api.Controllers;

/// <summary>
/// 菜单管理控制器，提供菜单的增删改查及树形结构 REST 接口。
/// </summary>
[Authorize]
[ApiController]
[Route("api/menus")]
public class MenusController(IMenuService service, ICacheService cache) : ControllerBase
{
    /// <summary>获取所有菜单扁平列表，按排序号升序排列。</summary>
    // GET /api/menus
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var cached = await cache.GetAsync("menus:all");
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var menus = await service.GetAllAsync();
        var data = menus.Select(ToResponse).ToList();
        await cache.SetAsync("menus:all", JsonSerializer.Serialize(data), "Menus");
        return Ok(data);
    }

    /// <summary>获取所有菜单并组装为树形结构，从根节点（Pid=0）开始递归构建。</summary>
    // GET /api/menus/tree
    [HttpGet("tree")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree()
    {
        var cached = await cache.GetAsync("menus:tree");
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var all = (await service.GetAllAsync()).ToList();
        var tree = BuildTree(all, 0);
        await cache.SetAsync("menus:tree", JsonSerializer.Serialize(tree), "Menus");
        return Ok(tree);
    }

    /// <summary>按 ID 获取单个菜单。</summary>
    // GET /api/menus/{id}
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var cached = await cache.GetAsync($"menus:{id}");
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var menu = await service.GetByIdAsync(id);
        if (menu is null) return NotFound();

        var data = ToResponse(menu);
        await cache.SetAsync($"menus:{id}", JsonSerializer.Serialize(data), "Menus");
        return Ok(data);
    }

    /// <summary>新增菜单，ID 由服务端雪花算法自动生成。</summary>
    // POST /api/menus
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] MenuRequest req)
    {
        var created = await service.CreateAsync(ToEntity(req));
        await cache.RemoveAsync("menus:all", "menus:tree");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
    }

    /// <summary>全量更新指定 ID 的菜单信息。</summary>
    // PUT /api/menus/{id}
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(long id, [FromBody] MenuRequest req)
    {
        var updated = await service.UpdateAsync(id, ToEntity(req));
        if (updated is null) return NotFound();
        await cache.RemoveAsync("menus:all", "menus:tree", $"menus:{id}");
        return Ok(ToResponse(updated));
    }

    /// <summary>
    /// 删除指定 ID 的菜单。若菜单下存在子菜单，返回 400 阻止删除。
    /// </summary>
    // DELETE /api/menus/{id}
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        if (await service.HasChildrenAsync(id))
            return BadRequest(new { message = "该菜单下存在子菜单，请先删除子菜单。" });

        var deleted = await service.DeleteAsync(id);
        if (!deleted) return NotFound();
        await cache.RemoveAsync("menus:all", "menus:tree", $"menus:{id}");
        return NoContent();
    }

    private static Menu ToEntity(MenuRequest req) => new()
    {
        Pid         = req.Pid,
        Type        = req.Type,
        Name        = req.Name,
        Path        = req.Path,
        Component   = req.Component,
        Redirect    = req.Redirect,
        Permission  = req.Permission,
        Title       = req.Title,
        Icon        = req.Icon,
        IsIframe    = req.IsIframe,
        OutLink     = req.OutLink,
        IsHide      = req.IsHide,
        IsKeepAlive = req.IsKeepAlive,
        IsAffix     = req.IsAffix,
        OrderNo     = req.OrderNo,
        Status      = req.Status,
        Remark      = req.Remark
    };

    private static object ToResponse(Menu m) => new
    {
        id          = m.Id.ToString(),
        pid         = m.Pid.ToString(),
        type        = (int)m.Type,
        name        = m.Name,
        path        = m.Path,
        component   = m.Component,
        redirect    = m.Redirect,
        permission  = m.Permission,
        title       = m.Title,
        icon        = m.Icon,
        isIframe    = m.IsIframe,
        outLink     = m.OutLink,
        isHide      = m.IsHide,
        isKeepAlive = m.IsKeepAlive,
        isAffix     = m.IsAffix,
        orderNo     = m.OrderNo,
        status      = (int)m.Status,
        remark      = m.Remark
    };

    private static List<object> BuildTree(List<Menu> all, long pid)
    {
        return all
            .Where(m => m.Pid == pid)
            .OrderBy(m => m.OrderNo)
            .Select(m => (object)new
            {
                id          = m.Id.ToString(),
                pid         = m.Pid.ToString(),
                type        = (int)m.Type,
                name        = m.Name,
                path        = m.Path,
                component   = m.Component,
                redirect    = m.Redirect,
                permission  = m.Permission,
                title       = m.Title,
                icon        = m.Icon,
                isIframe    = m.IsIframe,
                outLink     = m.OutLink,
                isHide      = m.IsHide,
                isKeepAlive = m.IsKeepAlive,
                isAffix     = m.IsAffix,
                orderNo     = m.OrderNo,
                status      = (int)m.Status,
                remark      = m.Remark,
                children    = BuildTree(all, m.Id)
            })
            .ToList();
    }
}
