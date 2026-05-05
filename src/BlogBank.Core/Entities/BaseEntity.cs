using System.ComponentModel.DataAnnotations;

namespace BlogBank.Core.Entities;

public abstract class BaseEntity
{
    /// <summary>
    /// 文章唯一标识，由雪花算法生成的 64 位整数。
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 创建时间（UTC）。
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 最后更新时间（UTC）。
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public bool IsDeleted { get; set; }
    
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } // 乐观锁，防止并发冲突
}