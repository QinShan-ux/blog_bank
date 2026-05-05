using BlogBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogBank.Infrastructure.Data.Configurations;

public class ArticleTagConfiguration: IEntityTypeConfiguration<ArticleTag>
{
    public void Configure(EntityTypeBuilder<ArticleTag> entity)
    {
        entity.HasKey(t => t.Id);
        entity.ToTable("article_tags", t => t.HasComment("文章标签"));

        // entity.Property(t => t.Id)
        //       .HasComment("标签记录自增主键");
        entity.Property(t => t.ArticleId)
            .HasComment("所属文章 ID（外键）");
        entity.Property(t => t.Tag)
            .HasMaxLength(100).IsRequired()
            .HasComment("标签名称，例如：CSS、响应式");
    }
}