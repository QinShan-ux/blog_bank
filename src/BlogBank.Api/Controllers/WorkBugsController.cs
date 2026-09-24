using System.Text.Json;
using BlogBank.Api.Filters;
using BlogBank.Api.Models;
using BlogBank.Core.Entities;
using BlogBank.Core.Enums;
using BlogBank.Core.Interfaces;
using BlogBank.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogBank.Api.Controllers;

/// <summary>
/// 工作 Bug 记录控制器，提供工作中遇到的 bug 及处理方法的增删改查 REST 接口。
/// </summary>
[Authorize]
[ApiController]
[Route("api/workbugs")]
public class WorkBugsController(IWorkBugService service, ICacheService cache) : ControllerBase
{
    /// <summary>获取 bug 列表，按发生日期倒序；支持按状态/严重程度/项目/关键字筛选，无筛选参数时走缓存。</summary>
    // GET /api/workbugs?status=0&severity=2&project=订单系统&keyword=超时
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] BugStatusEnum? status,
        [FromQuery] BugSeverityEnum? severity,
        [FromQuery] string? project,
        [FromQuery] string? keyword)
    {
        var filtered = status.HasValue || severity.HasValue
                       || !string.IsNullOrWhiteSpace(project) || !string.IsNullOrWhiteSpace(keyword);

        // 带筛选条件的结果不走缓存，只缓存全量列表
        if (!filtered)
        {
            var cached = await cache.GetAsync("workbugs:all");
            if (cached != null)
                return Ok(JsonSerializer.Deserialize<JsonElement>(cached));
        }

        var bugs = await service.GetAllAsync(status, severity, project, keyword);
        var data = bugs.Select(ToResponse).ToList();

        if (!filtered)
            await cache.SetAsync("workbugs:all", JsonSerializer.Serialize(data), "WorkBugs");

        return Ok(data);
    }

    /// <summary>按 ID 获取单条 bug 记录。</summary>
    // GET /api/workbugs/{id}
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var cached = await cache.GetAsync($"workbugs:{id}");
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var bug = await service.GetByIdAsync(id);
        if (bug is null) return NotFound();

        var data = ToResponse(bug);
        await cache.SetAsync($"workbugs:{id}", JsonSerializer.Serialize(data), "WorkBugs");
        return Ok(data);
    }

    /// <summary>新增一条 bug 记录，ID 由服务端雪花算法自动生成；状态为已解决且未传解决日期时自动取当天。</summary>
    // POST /api/workbugs
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Audit]
    public async Task<IActionResult> Create([FromBody] WorkBugRequest req)
    {
        var created = await service.CreateAsync(ToEntity(req));
        await cache.RemoveAsync("workbugs:all");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
    }

    /// <summary>全量更新指定 ID 的 bug 记录；状态为已解决且未传解决日期时自动取当天。</summary>
    // PUT /api/workbugs/{id}
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(long id, [FromBody] WorkBugRequest req)
    {
        var updated = await service.UpdateAsync(id, ToEntity(req));
        if (updated is null) return NotFound();
        await cache.RemoveAsync("workbugs:all", $"workbugs:{id}");
        return Ok(ToResponse(updated));
    }

    /// <summary>删除指定 ID 的 bug 记录。</summary>
    // DELETE /api/workbugs/{id}
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await service.DeleteAsync(id);
        if (!deleted) return NotFound();
        await cache.RemoveAsync("workbugs:all", $"workbugs:{id}");
        return NoContent();
    }

    private static WorkBug ToEntity(WorkBugRequest req) => new()
    {
        Title        = req.Title,
        Description  = req.Description,
        RootCause    = req.RootCause,
        Solution     = req.Solution,
        Severity     = (BugSeverityEnum)req.Severity,
        Status       = (BugStatusEnum)req.Status,
        Project      = req.Project,
        Environment  = req.Environment,
        OccurredDate = DateOnly.Parse(req.OccurredDate),
        // 状态为已解决但未传解决日期时，自动取当天
        SolvedDate   = (BugStatusEnum)req.Status == BugStatusEnum.Resolved
            ? (string.IsNullOrWhiteSpace(req.SolvedDate) ? DateOnly.FromDateTime(DateTime.Now) : DateOnly.Parse(req.SolvedDate))
            : (string.IsNullOrWhiteSpace(req.SolvedDate) ? null : DateOnly.Parse(req.SolvedDate))
    };

    private static object ToResponse(WorkBug b) => new
    {
        id           = b.Id.ToString(),
        title        = b.Title,
        description  = b.Description,
        rootCause    = b.RootCause,
        solution     = b.Solution,
        severity     = (int)b.Severity,
        status       = (int)b.Status,
        project      = b.Project,
        environment  = b.Environment,
        occurredDate = b.OccurredDate.ToString("yyyy-MM-dd"),
        solvedDate   = b.SolvedDate?.ToString("yyyy-MM-dd"),
        createdAt    = b.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
        updatedAt    = b.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
    };
}
