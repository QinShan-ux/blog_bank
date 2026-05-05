namespace BlogBank.Core.Entities;

/// <summary>
/// 文章标签实体，与 <see cref="Article"/> 为多对一关系。
/// </summary>
public class ArticleTag: BaseEntity
{

    /// <summary>
    /// 所属文章的 ID（外键）。
    /// </summary>
    public long ArticleId { get; set; }

    /// <summary>
    /// 标签名称，例如"CSS"、"响应式"。
    /// </summary>
    public string Tag { get; set; } = string.Empty;

    /// <summary>
    /// 所属文章的导航属性。
    /// </summary>
    public Article Article { get; set; } = null!;
}
