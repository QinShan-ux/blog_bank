namespace BlogBank.Core.Enums;

/// <summary>
/// 工作 Bug 严重程度枚举。
/// </summary>
public enum BugSeverityEnum
{
    /// <summary>低，不影响主要功能，可择期处理。</summary>
    Low = 0,

    /// <summary>中，影响部分功能但存在临时绕过方案。</summary>
    Medium = 1,

    /// <summary>高，核心功能不可用或造成数据错误，需立即处理。</summary>
    High = 2
}
