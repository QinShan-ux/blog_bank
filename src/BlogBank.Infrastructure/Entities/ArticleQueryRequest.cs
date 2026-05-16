namespace BlogBank.Infrastructure.Entities;

public class ArticleQueryRequest
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsDelete { get; set; }
    
}