using BlogBank.Core.Enums;

namespace BlogBank.Core.Entities;

/// <summary>
/// 工作 Bug 记录实体，用于记录工作上遇到的 bug 及处理方法。
/// </summary>
public class WorkBug : BaseEntity
{
    /// <summary>
    /// bug 标题，简明概括问题。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// bug 现象描述，包含报错信息、复现步骤等。
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 原因分析，排查后定位到的根本原因；未定位时可为空。
    /// </summary>
    public string? RootCause { get; set; }

    /// <summary>
    /// 处理方法（HTML 格式），修复方案或临时绕过方式，可含代码块、加粗、列表等标签。
    /// </summary>
    public string Solution { get; set; } = string.Empty;

    /// <summary>
    /// 严重程度。
    /// </summary>
    public BugSeverityEnum Severity { get; set; } = BugSeverityEnum.Low;

    /// <summary>
    /// 处理状态。
    /// </summary>
    public BugStatusEnum Status { get; set; } = BugStatusEnum.Pending;

    /// <summary>
    /// 所属项目/模块，例如"订单系统"。
    /// </summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// 环境，自由文本，例如"生产环境"、"客户环境 v2.3"。
    /// </summary>
    public string? Environment { get; set; }

    /// <summary>
    /// 发生日期。
    /// </summary>
    public DateOnly OccurredDate { get; set; }

    /// <summary>
    /// 解决日期，未解决时为空。
    /// </summary>
    public DateOnly? SolvedDate { get; set; }
}
