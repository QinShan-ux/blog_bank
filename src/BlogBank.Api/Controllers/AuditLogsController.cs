using BlogBank.Api.Models;
using BlogBank.Core.Entities;
using BlogBank.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogBank.Api.Controllers;

/// <summary>
/// 操作日志控制器，提供日志的查询、新增和删除接口。
/// </summary>
[Authorize]
[ApiController]
[Route("api/audit-logs")]
public class AuditLogsController(IAuditLogService service) : ControllerBase
{
    /// <summary>
    /// 分页查询操作日志，支持按操作人、操作类型、表名、时间范围过滤。
    /// </summary>
    // GET /api/audit-logs?page=1&pageSize=20&userId=&action=&tableName=&startTime=&endTime=
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? tableName = null,
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var (items, total) = await service.GetPagedAsync(page, pageSize, userId, action, tableName, startTime, endTime);
        return Ok(new { total, page, pageSize, items });
    }

    /// <summary>按 ID 获取单条操作日志。</summary>
    // GET /api/audit-logs/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var log = await service.GetByIdAsync(id);
        if (log is null) return NotFound();
        return Ok(log);
    }

    /// <summary>手动新增一条操作日志，操作时间由服务端自动记录。</summary>
    // POST /api/audit-logs
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] AuditLogRequest req)
    {
        var created = await service.CreateAsync(ToEntity(req));
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>删除指定 ID 的操作日志。</summary>
    // DELETE /api/audit-logs/{id}
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await service.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    private static AuditLog ToEntity(AuditLogRequest req) => new()
    {
        UserId     = req.UserId,
        UserName   = req.UserName,
        Action     = req.Action,
        TableName  = req.TableName,
        EntityId   = req.EntityId,
        OldValues  = req.OldValues,
        NewValues  = req.NewValues,
        RequestUrl = req.RequestUrl,
        IpAddress  = req.IpAddress
    };
}
