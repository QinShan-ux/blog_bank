using BlogBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogBank.Infrastructure.Data.Configurations;

public class UserRoleConfiguration: IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> entity)
    {
        entity.HasKey(ur => new { ur.UserId, ur.RoleId });
        entity.ToTable("user_roles", t => t.HasComment("用户角色关联"));

        entity.Property(ur => ur.UserId)
            .HasComment("关联的用户 ID（联合主键之一）");
        entity.Property(ur => ur.RoleId)
            .HasComment("关联的角色 ID（联合主键之一）");
        entity.Property(ur => ur.AssignedAt)
            .HasComment("授权时间");

        entity.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}