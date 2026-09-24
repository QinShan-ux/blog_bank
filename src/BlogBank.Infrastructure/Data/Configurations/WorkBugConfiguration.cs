using BlogBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogBank.Infrastructure.Data.Configurations;

public class WorkBugConfiguration : IEntityTypeConfiguration<WorkBug>
{
    public void Configure(EntityTypeBuilder<WorkBug> entity)
    {
        entity.HasKey(e => e.Id);
        entity.ToTable("work_bugs", t => t.HasComment("工作 Bug 记录"));

        entity.Property(e => e.Title)
            .HasMaxLength(500).IsRequired()
            .HasComment("bug 标题，简明概括问题");
        entity.Property(e => e.Description)
            .IsRequired()
            .HasComment("bug 现象描述，包含报错信息、复现步骤等");
        entity.Property(e => e.RootCause)
            .HasComment("原因分析，排查后定位到的根本原因；未定位时可为空");
        entity.Property(e => e.Solution)
            .IsRequired()
            .HasComment("处理方法（HTML 格式），修复方案或临时绕过方式，可含代码块、加粗、列表等标签");
        entity.Property(e => e.Severity)
            .HasConversion<int>()
            .HasComment("严重程度：0=低 1=中 2=高");
        entity.Property(e => e.Status)
            .HasConversion<int>()
            .HasComment("处理状态：0=待处理 1=处理中 2=已解决");
        entity.Property(e => e.Project)
            .HasMaxLength(200).IsRequired()
            .HasComment("所属项目/模块，例如：订单系统");
        entity.Property(e => e.Environment)
            .HasMaxLength(100)
            .HasComment("环境，自由文本，例如：生产环境、客户环境 v2.3");
        entity.Property(e => e.OccurredDate)
            .HasComment("发生日期");
        entity.Property(e => e.SolvedDate)
            .HasComment("解决日期，未解决时为空");
    }
}
