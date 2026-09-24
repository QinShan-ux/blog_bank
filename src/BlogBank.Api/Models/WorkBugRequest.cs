using System.ComponentModel.DataAnnotations;

namespace BlogBank.Api.Models;

/// <summary>
/// 新增或更新工作 Bug 记录的请求体。
/// </summary>
/// <param name="Title">bug 标题，简明概括问题。</param>
/// <param name="Description">bug 现象描述，包含报错信息、复现步骤等。</param>
/// <param name="RootCause">原因分析，未定位时可传 null。</param>
/// <param name="Solution">处理方法（HTML 格式），可含代码块、加粗、列表等标签，原样存储与返回。</param>
/// <param name="Severity">严重程度：0=低 1=中 2=高。</param>
/// <param name="Status">处理状态：0=待处理 1=处理中 2=已解决。</param>
/// <param name="Project">所属项目/模块，例如"订单系统"。</param>
/// <param name="Environment">环境，例如"生产环境"。</param>
/// <param name="OccurredDate">发生日期，格式 <c>yyyy-MM-dd</c>。</param>
/// <param name="SolvedDate">解决日期，格式 <c>yyyy-MM-dd</c>；未解决传 null，状态为已解决时缺省自动取当天。</param>
public record WorkBugRequest(
    [Required] string Title,
    [Required] string Description,
    string? RootCause,
    [Required] string Solution,
    int Severity,
    int Status,
    [Required] string Project,
    string? Environment,
    [Required] string OccurredDate,
    string? SolvedDate
);
