using BlogBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogBank.Infrastructure.Data.Configurations;

public class UserMenuConfiguration: IEntityTypeConfiguration<UserMenu>
{
    public void Configure(EntityTypeBuilder<UserMenu> entity)
    {
        entity.HasKey(um => new { um.UserId, um.MenuId });
        entity.ToTable("user_menus", t => t.HasComment("用户菜单关联"));

        entity.Property(um => um.UserId)
            .HasComment("关联的用户 ID（联合主键之一）");
        entity.Property(um => um.MenuId)
            .HasComment("关联的菜单 ID（联合主键之一）");
        entity.Property(um => um.AssignedAt)
            .HasComment("授权时间");

        entity.HasOne(um => um.User)
            .WithMany(u => u.UserMenus)
            .HasForeignKey(um => um.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(um => um.Menu)
            .WithMany(m => m.UserMenus)
            .HasForeignKey(um => um.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}