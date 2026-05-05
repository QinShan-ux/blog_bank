namespace BlogBank.Core.Entities;

/// <summary>
/// 博客文章实体。
/// </summary>
public class Article: BaseEntity
{
    

    /// <summary>
    /// 文章标题。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 发布日期。
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// 所属分类，例如"前端开发"、"JavaScript"、"DevOps"。
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 预计阅读时长，例如"5 分钟阅读"。
    /// </summary>
    public string ReadTime { get; set; } = string.Empty;

    /// <summary>
    /// 文章摘要，用于列表页展示。
    /// </summary>
    public string Excerpt { get; set; } = string.Empty;

    /// <summary>
    /// 文章正文，HTML 格式字符串。
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 标签列表，用于筛选过滤。
    /// </summary>
    public List<ArticleTag> Tags { get; set; } = [];
}
