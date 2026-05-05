using BlogBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogBank.Infrastructure.Data.Configurations;

public class ArticleConfigurations: IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> entity)
    {
        entity.HasKey(a => a.Id);
        entity.ToTable("articles", t => t.HasComment("博客文章"));

        // entity.Property(a => a.Id)
        //       .ValueGeneratedNever()
        //       .HasComment("文章唯一标识（雪花算法生成）");
        entity.Property(a => a.Title)
            .HasMaxLength(500).IsRequired()
            .HasComment("文章标题");
        entity.Property(a => a.Date)
            .HasComment("发布日期");
        entity.Property(a => a.Category)
            .HasMaxLength(100).IsRequired()
            .HasComment("所属分类，例如：前端开发、JavaScript、DevOps");
        entity.Property(a => a.ReadTime)
            .HasMaxLength(50).IsRequired()
            .HasComment("预计阅读时长，例如：5 分钟阅读");
        entity.Property(a => a.Excerpt)
            .HasMaxLength(1000).IsRequired()
            .HasComment("文章摘要，用于列表页展示");
        entity.Property(a => a.Content)
            .IsRequired()
            .HasComment("文章正文（HTML 格式）");

        entity.HasMany(a => a.Tags)
            .WithOne(t => t.Article)
            .HasForeignKey(t => t.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}