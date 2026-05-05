using BlogBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogBank.Infrastructure.Data.Configurations;

public class EssayConfiguration: IEntityTypeConfiguration<Essay>
{
    public void Configure(EntityTypeBuilder<Essay> entity)
    {
        entity.HasKey(e => e.Id);
        entity.ToTable("essays", t => t.HasComment("随笔"));

        // entity.Property(e => e.Id)
        //       .ValueGeneratedNever()
        //       .HasComment("随笔唯一标识（雪花算法生成）");
        entity.Property(e => e.Title)
            .HasMaxLength(500).IsRequired()
            .HasComment("随笔标题");
        entity.Property(e => e.Date)
            .HasComment("发布日期");
        entity.Property(e => e.Mood)
            .HasMaxLength(50).IsRequired()
            .HasComment("心情文字描述，例如：惬意、专注");
        entity.Property(e => e.MoodIcon)
            .HasMaxLength(10).IsRequired()
            .HasComment("心情 Emoji 图标，例如：😌");
        entity.Property(e => e.Weather)
            .HasMaxLength(50).IsRequired()
            .HasComment("天气文字描述，例如：晴、多云");
        entity.Property(e => e.WeatherIcon)
            .HasMaxLength(10).IsRequired()
            .HasComment("天气 Emoji 图标，例如：☀️");
        entity.Property(e => e.Location)
            .HasMaxLength(200).IsRequired()
            .HasComment("写作地点，例如：街角咖啡馆");
        entity.Property(e => e.Excerpt)
            .HasMaxLength(1000).IsRequired()
            .HasComment("随笔摘要，用于列表页折叠展示");
        entity.Property(e => e.Content)
            .IsRequired()
            .HasComment("随笔正文（纯文本）");
        entity.Property(e => e.BgColor)
            .HasMaxLength(20).IsRequired()
            .HasComment("卡片背景色，十六进制颜色值，例如：#fef3c7");

        entity.HasMany(e => e.Tags)
            .WithOne(t => t.Essay)
            .HasForeignKey(t => t.EssayId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}