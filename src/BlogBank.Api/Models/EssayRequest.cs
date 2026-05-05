using System.ComponentModel.DataAnnotations;

namespace BlogBank.Api.Models;

/// <summary>
/// 新增或更新随笔的请求体。
/// </summary>
/// <param name="Title">随笔标题。</param>
/// <param name="Date">发布日期，格式 <c>yyyy-MM-dd</c>。</param>
/// <param name="Mood">心情文字描述，例如"惬意"、"专注"。</param>
/// <param name="MoodIcon">心情 Emoji 图标，例如"😌"。</param>
/// <param name="Weather">天气文字描述，例如"晴"、"多云"。</param>
/// <param name="WeatherIcon">天气 Emoji 图标，例如"☀️"。</param>
/// <param name="Location">写作地点，例如"街角咖啡馆"。</param>
/// <param name="Tags">标签列表，用于筛选过滤。</param>
/// <param name="Excerpt">随笔摘要，用于列表页折叠展示。</param>
/// <param name="Content">随笔正文，纯文本格式。</param>
/// <param name="BgColor">卡片背景色，十六进制颜色值，例如"#fef3c7"。</param>
public record EssayRequest(
    [Required] string Title,
    [Required] string Date,
    [Required] string Mood,
    [Required] string MoodIcon,
    [Required] string Weather,
    [Required] string WeatherIcon,
    [Required] string Location,
    [Required] List<string> Tags,
    [Required] string Excerpt,
    [Required] string Content,
    [Required] string BgColor
);
