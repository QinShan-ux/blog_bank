namespace BlogBank.Core.Entities;

public class AuditLog
{
    public long Id { get; set; }

    // HTTP 层信息（来自 ActionFilter）
    public string TraceId { get; set; }      // 追踪ID，关联 HTTP 和数据层日志
    public string UserId { get; set; }       // 操作人ID
    public string UserName { get; set; }     // 操作人姓名
    public string RequestUrl { get; set; }   // 请求接口地址
    public string IpAddress { get; set; }    // 客户端IP
    public string HttpMethod { get; set; }   // GET/POST/PUT/DELETE

    // 数据层信息（来自 ChangeTracker）
    public string TableName { get; set; }    // 操作的表名
    public string EntityId { get; set; }     // 操作的数据ID
    public string Action { get; set; }       // 新增/修改/删除
    public string OldValues { get; set; }    // 修改前的值（JSON）
    public string NewValues { get; set; }    // 修改后的值（JSON）

    // 时间
    public DateTime OperatedAt { get; set; }
}
