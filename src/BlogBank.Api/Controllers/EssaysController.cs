using System.Text.Json;
using BlogBank.Api.Filters;
using BlogBank.Api.Models;
using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogBank.Api.Controllers;

/// <summary>
/// 随笔管理控制器，提供随笔的增删改查 REST 接口。
/// </summary>
[Authorize]
[ApiController]
[Route("api/essays")]
public class EssaysController(IEssayService service, ICacheService cache) : ControllerBase
{
    /// <summary>获取所有随笔列表，按发布日期倒序排列。</summary>
    // GET /api/essays
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var cached = await cache.GetAsync("essays:all");
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var essays = await service.GetAllAsync();
        var data = essays.Select(ToResponse).ToList();
        await cache.SetAsync("essays:all", JsonSerializer.Serialize(data), "Essays");
        return Ok(data);
    }

    /// <summary>按 ID 获取单篇随笔。</summary>
    // GET /api/essays/{id}
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var cached = await cache.GetAsync($"essays:{id}");
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var essay = await service.GetByIdAsync(id);
        if (essay is null) return NotFound();

        var data = ToResponse(essay);
        await cache.SetAsync($"essays:{id}", JsonSerializer.Serialize(data), "Essays");
        return Ok(data);
    }

    /// <summary>新增一篇随笔，ID 由服务端雪花算法自动生成。</summary>
    // POST /api/essays
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Audit]
    public async Task<IActionResult> Create([FromBody] EssayRequest req)
    {
        var created = await service.CreateAsync(ToEntity(req));
        await cache.RemoveAsync("essays:all");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
    }

    /// <summary>批量新增随笔，所有记录在同一事务中写入，返回已创建的随笔列表。</summary>
    // POST /api/essays/batch
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBatch([FromBody] List<EssayRequest> reqs)
    {
        if (reqs.Count == 0)
            return BadRequest(new { message = "请求列表不能为空。" });

        var created = await service.CreateBatchAsync(reqs.Select(ToEntity));
        await cache.RemoveAsync("essays:all");
        return StatusCode(StatusCodes.Status201Created, created.Select(ToResponse));
    }

    /// <summary>全量更新指定 ID 的随笔内容及标签。</summary>
    // PUT /api/essays/{id}
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(long id, [FromBody] EssayRequest req)
    {
        var updated = await service.UpdateAsync(id, ToEntity(req));
        if (updated is null) return NotFound();
        await cache.RemoveAsync("essays:all", $"essays:{id}");
        return Ok(ToResponse(updated));
    }

    /// <summary>删除指定 ID 的随笔及其所有标签。</summary>
    // DELETE /api/essays/{id}
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await service.DeleteAsync(id);
        if (!deleted) return NotFound();
        await cache.RemoveAsync("essays:all", $"essays:{id}");
        return NoContent();
    }

    private static Essay ToEntity(EssayRequest req) => new()
    {
        Title       = req.Title,
        Date        = DateOnly.Parse(req.Date),
        Mood        = req.Mood,
        MoodIcon    = req.MoodIcon,
        Weather     = req.Weather,
        WeatherIcon = req.WeatherIcon,
        Location    = req.Location,
        Excerpt     = req.Excerpt,
        Content     = req.Content,
        BgColor     = req.BgColor,
        Tags        = req.Tags.Select(t => new EssayTag { Tag = t }).ToList()
    };

    private static object ToResponse(Essay e) => new
    {
        id          = e.Id.ToString(),
        title       = e.Title,
        date        = e.Date.ToString("yyyy-MM-dd"),
        mood        = e.Mood,
        moodIcon    = e.MoodIcon,
        weather     = e.Weather,
        weatherIcon = e.WeatherIcon,
        location    = e.Location,
        tags        = e.Tags.Select(t => t.Tag).ToList(),
        excerpt     = e.Excerpt,
        content     = e.Content,
        bgColor     = e.BgColor
    };
}
