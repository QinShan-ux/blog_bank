namespace BlogBank.Core.Entities;

/// <summary>
/// 随笔标签实体，与 <see cref="Essay"/> 为多对一关系。
/// </summary>
public class EssayTag: BaseEntity
{
    /// <summary>
    /// 所属随笔的 ID（外键）。
    /// </summary>
    public long EssayId { get; set; }

    /// <summary>
    /// 标签名称，例如"生活"、"咖啡"。
    /// </summary>
    public string Tag { get; set; } = string.Empty;

    /// <summary>
    /// 所属随笔的导航属性。
    /// </summary>
    public Essay Essay { get; set; } = null!;
}
