namespace BlogBank.Core.dto;

public class AuditHttpInfo
{
    public string TraceId { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string RequestUrl { get; set; }
    public string IpAddress { get; set; }
    public string HttpMethod { get; set; }
    public DateTime OperatedAt { get; set; }
}