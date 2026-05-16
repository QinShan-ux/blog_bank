using BlogBank.Core.Entities.Mq;

namespace BlogBank.Infrastructure.Entities;

public class ExportJob: MyMessage
{
    public long TaskId { get; set; }
    
    public string UserId { get; set; }
    
    public ArticleQueryRequest QueryRequest { get; set; }
}