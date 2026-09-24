namespace BlogBank.Core.Enums;

/// <summary>
/// 工作 Bug 处理状态枚举。
/// </summary>
public enum BugStatusEnum
{
    /// <summary>待处理，问题刚发现还未开始排查。</summary>
    Pending = 0,

    /// <summary>处理中，正在排查或修复。</summary>
    Processing = 1,

    /// <summary>已解决，bug 已修复并验证通过。</summary>
    Resolved = 2
}
