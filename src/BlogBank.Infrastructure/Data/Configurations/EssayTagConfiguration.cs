using BlogBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogBank.Infrastructure.Data.Configurations;

public class EssayTagConfiguration: IEntityTypeConfiguration<EssayTag>
{
    public void Configure(EntityTypeBuilder<EssayTag> entity)
    {
        entity.HasKey(t => t.Id);
        entity.ToTable("essay_tags", t => t.HasComment("随笔标签"));

        // entity.Property(t => t.Id)
        //       .HasComment("标签记录自增主键");
        entity.Property(t => t.EssayId)
            .HasComment("所属随笔 ID（外键）");
        entity.Property(t => t.Tag)
            .HasMaxLength(100).IsRequired()
            .HasComment("标签名称，例如：生活、咖啡");
    }
}