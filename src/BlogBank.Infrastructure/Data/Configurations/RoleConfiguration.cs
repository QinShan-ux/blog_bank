using BlogBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogBank.Infrastructure.Data.Configurations;

public class RoleConfiguration: IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.HasKey(r => r.Id);
        entity.ToTable("roles", t => t.HasComment("角色"));

        // entity.Property(r => r.Id)
        //       .HasComment("角色自增主键");
        entity.Property(r => r.Code)
            .HasMaxLength(50).IsRequired()
            .HasComment("角色编码，程序内部使用，全局唯一，例如：admin、editor");
        entity.Property(r => r.Name)
            .HasMaxLength(100).IsRequired()
            .HasComment("角色显示名称，例如：管理员、编辑者");
        entity.Property(r => r.Description)
            .HasMaxLength(500).IsRequired()
            .HasComment("角色描述，说明该角色的权限范围");
        entity.Property(r => r.CreatedAt)
            .HasComment("创建时间");

        entity.HasIndex(r => r.Code).IsUnique();
    }
}