using BlogBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BlogBank.Infrastructure.Data.Configurations;

public class ExportConfiguration:IEntityTypeConfiguration<ExportTask>
{
    public void Configure(EntityTypeBuilder<ExportTask> entity)
    {
        entity.HasKey(a => a.Id);
        entity.ToTable("exportTask", t => t.HasComment("导出任务"));
        entity.Property(a => a.Status)
            .HasComment("状态");
        entity.Property(a => a.Progress)
            .HasComment("进度");
        entity.Property(a => a.FileUrl)
            .HasMaxLength(500)
            .HasComment("文件地址");

        entity.Property(a => a.ErrorMessage)
            .HasMaxLength(500)
            .HasComment("错误信息");
        entity.Property(a => a.OperatorId)
            .HasMaxLength(500)
            .HasComment("操作人id");
        entity.Property(a => a.CompletedAt)
            .HasComment("完成时间");
    }
}