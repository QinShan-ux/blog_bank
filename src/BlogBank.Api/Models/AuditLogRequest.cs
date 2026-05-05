using System.ComponentModel.DataAnnotations;

namespace BlogBank.Api.Models;

public record AuditLogRequest(
    [Required][MaxLength(100)] string UserId,
    [Required][MaxLength(100)] string UserName,
    [Required][MaxLength(10)]  string Action,
    [Required][MaxLength(100)] string TableName,
    [MaxLength(100)] string EntityId = "",
    string OldValues = "",
    string NewValues = "",
    [MaxLength(500)] string RequestUrl = "",
    [MaxLength(50)]  string IpAddress = ""
);
