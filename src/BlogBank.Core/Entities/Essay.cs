namespace BlogBank.Core.Entities;

/// <summary>
/// 随笔实体。
/// </summary>
public class Essay: BaseEntity
{

    /// <summary>
    /// 随笔标题。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 发布日期。
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// 心情文字描述，例如"惬意"、"专注"。
    /// </summary>
    public string Mood { get; set; } = string.Empty;

    /// <summary>
    /// 心情 Emoji 图标，例如"😌"。
    /// </summary>
    public string MoodIcon { get; set; } = string.Empty;

    /// <summary>
    /// 天气文字描述，例如"晴"、"多云"。
    /// </summary>
    public string Weather { get; set; } = string.Empty;

    /// <summary>
    /// 天气 Emoji 图标，例如"☀️"。
    /// </summary>
    public string WeatherIcon { get; set; } = string.Empty;

    /// <summary>
    /// 写作地点，例如"街角咖啡馆"。
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// 随笔摘要，用于列表页折叠展示。
    /// </summary>
    public string Excerpt { get; set; } = string.Empty;

    /// <summary>
    /// 随笔正文，纯文本格式。
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 卡片背景色，十六进制颜色值，例如"#fef3c7"。
    /// </summary>
    public string BgColor { get; set; } = string.Empty;

    /// <summary>
    /// 标签列表，用于筛选过滤。
    /// </summary>
    public List<EssayTag> Tags { get; set; } = [];
}
