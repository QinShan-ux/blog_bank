using BlogBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogBank.Infrastructure.Data.Configurations;

public class AuditLogConfiguration: IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
        entity.HasKey(l => l.Id);
        entity.ToTable("audit_logs", t => t.HasComment("操作审计日志"));

        entity.Property(l => l.UserId)
            .HasMaxLength(100).IsRequired()
            .HasComment("操作人ID");
        entity.Property(l => l.UserName)
            .HasMaxLength(100).IsRequired()
            .HasComment("操作人姓名");
        entity.Property(l => l.OperatedAt)
            .HasColumnType("datetime(6)")
            .HasComment("操作时间");
        entity.Property(l => l.Action)
            .HasMaxLength(10).IsRequired()
            .HasComment("操作类型：增/删/改/查");
        entity.Property(l => l.TableName)
            .HasMaxLength(100).IsRequired()
            .HasComment("操作的数据表名");
        entity.Property(l => l.EntityId)
            .HasMaxLength(100).IsRequired()
            .HasComment("操作的数据记录ID");
        entity.Property(l => l.OldValues)
            .HasComment("修改前的值（JSON格式）");
        entity.Property(l => l.NewValues)
            .HasComment("修改后的值（JSON格式）");
        entity.Property(l => l.RequestUrl)
            .HasMaxLength(500).IsRequired()
            .HasComment("请求接口地址");
        entity.Property(l => l.IpAddress)
            .HasMaxLength(50).IsRequired()
            .HasComment("客户端IP地址");

        entity.HasIndex(l => l.UserId).HasDatabaseName("IX_audit_logs_user_id");
        entity.HasIndex(l => l.OperatedAt).HasDatabaseName("IX_audit_logs_operated_at");
    }
}