using BlogBank.Core.Enums;

namespace BlogBank.Api.Filters;

[AttributeUsage(AttributeTargets.Property)]
public class SensitiveAttribute : Attribute
{
    public SensitiveType Type { get; }
    public string CustomPattern { get; set; }   // 自定义正则
    public string CustomReplace { get; set; }   // 自定义替换

    public SensitiveAttribute(SensitiveType type)
    {
        Type = type;
    }
    
    // Custom 类型专用构造函数
    public SensitiveAttribute(string pattern, string replacement)
    {
        Type = SensitiveType.Custom;
        CustomPattern = pattern;
        CustomReplace = replacement;
    }
}