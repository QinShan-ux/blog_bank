using BlogBank.Core.Enums;

namespace BlogBank.Core.Entities;

public class ExportTask: BaseEntity
{
    public TaskEnum Status { get; set; }   // Pending / Processing / Done / Failed
    public int Progress { get; set; }    // 0~100
    public string FileUrl { get; set; }  // 完成后的下载地址
    public string ErrorMessage { get; set; }
    public string OperatorId { get; set; }
    public DateTime? CompletedAt { get; set; }
}