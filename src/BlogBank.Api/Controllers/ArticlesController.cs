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
/// 文章管理控制器，提供文章的增删改查 REST 接口。
/// </summary>
[Authorize]
[ApiController]
[Route("api/articles")]
public class ArticlesController(IArticleService service, ICacheService cache) : ControllerBase
{
    /// <summary>获取所有文章列表，按发布日期倒序排列。</summary>
    // GET /api/articles
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var cached = await cache.GetAsync("articles:all");
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var articles = await service.GetAllAsync();
        var data = articles.Select(ToResponse).ToList();
        await cache.SetAsync("articles:all", JsonSerializer.Serialize(data), "Articles");
        return Ok(data);
    }

    /// <summary>获取所有文章的 ID 与标题列表，按发布日期倒序排列。</summary>
    // GET /api/articles/titles
    [HttpGet("titles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTitles()
    {
        var cached = await cache.GetAsync("articles:titles");
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var list = await service.GetIdTitleListAsync();
        var data = list.Select(x => new { id = x.Id.ToString(), title = x.Title }).ToList();
        await cache.SetAsync("articles:titles", JsonSerializer.Serialize(data), "Articles");
        return Ok(data);
    }

    /// <summary>按 ID 获取单篇文章。</summary>
    // GET /api/articles/{id}
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var cached = await cache.GetAsync($"articles:{id}");
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<JsonElement>(cached));

        var article = await service.GetByIdAsync(id);
        if (article is null) return NotFound();

        var data = ToResponse(article);
        await cache.SetAsync($"articles:{id}", JsonSerializer.Serialize(data), "Articles");
        return Ok(data);
    }

    /// <summary>新增一篇文章，ID 由服务端雪花算法自动生成。</summary>
    // POST /api/articles
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ArticleRequest req)
    {
        var created = await service.CreateAsync(ToEntity(req));
        await cache.RemoveAsync("articles:all", "articles:titles");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
    }

    /// <summary>批量新增文章，所有记录在同一事务中写入，返回已创建的文章列表。</summary>
    // POST /api/articles/batch
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBatch([FromBody] List<ArticleRequest> reqs)
    {
        if (reqs.Count == 0)
            return BadRequest(new { message = "请求列表不能为空。" });

        var created = await service.CreateBatchAsync(reqs.Select(ToEntity));
        await cache.RemoveAsync("articles:all", "articles:titles");
        return StatusCode(StatusCodes.Status201Created, created.Select(ToResponse));
    }

    /// <summary>全量更新指定 ID 的文章内容及标签。</summary>
    // PUT /api/articles/{id}
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Audit]
    public async Task<IActionResult> Update(long id, [FromBody] ArticleRequest req)
    {
        var updated = await service.UpdateAsync(id, ToEntity(req));
        if (updated is null) return NotFound();
        await cache.RemoveAsync("articles:all", "articles:titles", $"articles:{id}");
        return Ok(ToResponse(updated));
    }

    /// <summary>删除指定 ID 的文章及其所有标签。</summary>
    // DELETE /api/articles/{id}
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await service.DeleteAsync(id);
        if (!deleted) return NotFound();
        await cache.RemoveAsync("articles:all", "articles:titles", $"articles:{id}");
        return NoContent();
    }

    private static Article ToEntity(ArticleRequest req) => new()
    {
        Title = req.Title,
        Date = DateOnly.Parse(req.Date),
        Category = req.Category,
        ReadTime = req.ReadTime,
        Excerpt = req.Excerpt,
        Content = req.Content,
        Tags = req.Tags.Select(t => new ArticleTag { Tag = t }).ToList()
    };

    private static object ToResponse(Article a) => new
    {
        id = a.Id.ToString(),
        title = a.Title,
        date = a.Date.ToString("yyyy-MM-dd"),
        category = a.Category,
        readTime = a.ReadTime,
        excerpt = a.Excerpt,
        tags = a.Tags.Select(t => t.Tag).ToList(),
        content = a.Content
    };
}
