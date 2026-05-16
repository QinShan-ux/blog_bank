using BlogBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogBank.Infrastructure.Data.Configurations;

public class UserConfiguration: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(u => u.Id);
        entity.ToTable("users", t => t.HasComment("用户"));

        // entity.Property(u => u.Id)
        //       .ValueGeneratedNever()
        //       .HasComment("用户唯一标识（雪花算法生成）");
        entity.Property(u => u.Username)
            .HasMaxLength(50).IsRequired()
            .HasComment("用户名，用于登录，全局唯一");
        entity.Property(u => u.Nickname)
            .HasMaxLength(100).IsRequired()
            .HasComment("显示昵称");
        entity.Property(u => u.Phone)
            .HasMaxLength(20).IsRequired()
            .HasComment("电话");
        entity.Property(u => u.Email)
            .HasMaxLength(200).IsRequired()
            .HasComment("邮箱地址，全局唯一");
        entity.Property(u => u.Birthday)
            .HasComment("生日");
        entity.Property(u => u.Address)
            .HasMaxLength(500)
            .HasComment("地址");
        entity.Property(u => u.PasswordHash)
            .HasMaxLength(500).IsRequired()
            .HasComment("经过哈希处理的密码");
        entity.Property(u => u.Avatar)
            .HasMaxLength(500).IsRequired()
            .HasComment("头像 URL");
        entity.Property(u => u.IsEnabled)
            .HasComment("账号是否启用");
        entity.Property(u => u.CreatedAt)
            .HasComment("创建时间");
        entity.Property(u => u.UpdatedAt)
            .HasComment("最后更新时间");
        entity.Property(u => u.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
        entity.HasIndex(u => u.Username).IsUnique();
        entity.HasIndex(u => u.Email).IsUnique();
    }
}