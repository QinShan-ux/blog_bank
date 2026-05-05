using System.ComponentModel.DataAnnotations;

namespace BlogBank.Api.Models;

/// <summary>
/// 新增或更新文章的请求体。
/// </summary>
/// <param name="Title">文章标题。</param>
/// <param name="Date">发布日期，格式 <c>yyyy-MM-dd</c>。</param>
/// <param name="Category">所属分类，例如"前端开发"、"JavaScript"。</param>
/// <param name="ReadTime">预计阅读时长，例如"5 分钟阅读"。</param>
/// <param name="Excerpt">文章摘要，用于列表页展示。</param>
/// <param name="Tags">标签列表，用于筛选过滤。</param>
/// <param name="Content">文章正文，HTML 格式字符串。</param>
public record ArticleRequest(
    [Required] string Title,
    [Required] string Date,
    [Required] string Category,
    [Required] string ReadTime,
    [Required] string Excerpt,
    [Required] List<string> Tags,
    [Required] string Content
);
