using BlogBank.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogBank.Infrastructure.Data.Configurations;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> entity)
    {
        entity.HasKey(m => m.Id);
        entity.ToTable("menus", t => t.HasComment("系统菜单"));

        // entity.Property(m => m.Id)
        //       .ValueGeneratedNever()
        //       .HasComment("菜单唯一标识（雪花算法生成）");
        entity.Property(m => m.Pid)
            .HasComment("父菜单 ID，根菜单为 0");
        entity.Property(m => m.Type)
            .HasComment("菜单类型：1 目录、2 菜单、3 按钮")
            .HasConversion<int>();
        entity.Property(m => m.Name)
            .HasMaxLength(64)
            .HasComment("路由名称，对应前端 Vue Router name 字段");
        entity.Property(m => m.Path)
            .HasMaxLength(128)
            .HasComment("路由地址，对应前端 Vue Router path 字段");
        entity.Property(m => m.Component)
            .HasMaxLength(128)
            .HasComment("组件文件路径，相对于 views 目录");
        entity.Property(m => m.Redirect)
            .HasMaxLength(128)
            .HasComment("重定向地址，目录类型使用");
        entity.Property(m => m.Permission)
            .HasMaxLength(128)
            .HasComment("权限标识，格式如 sys:user:add");
        entity.Property(m => m.Title)
            .HasMaxLength(64).IsRequired()
            .HasComment("菜单显示名称");
        entity.Property(m => m.Icon)
            .HasMaxLength(128)
            .HasComment("菜单图标，格式如 ele-Menu");
        entity.Property(m => m.IsIframe)
            .HasComment("是否以内嵌 iframe 方式打开");
        entity.Property(m => m.OutLink)
            .HasMaxLength(256)
            .HasComment("外链 URL");
        entity.Property(m => m.IsHide)
            .HasComment("是否在侧边栏中隐藏");
        entity.Property(m => m.IsKeepAlive)
            .HasComment("是否开启页面缓存（keep-alive）");
        entity.Property(m => m.IsAffix)
            .HasComment("是否固定在标签栏，固定后不可关闭");
        entity.Property(m => m.OrderNo)
            .HasComment("排序号，数值越小越靠前");
        entity.Property(m => m.Status)
            .HasComment("菜单状态：0 禁用、1 启用")
            .HasConversion<int>();
        entity.Property(m => m.Remark)
            .HasMaxLength(256)
            .HasComment("备注");

        // Children 不映射到数据库
        entity.Ignore(m => m.Children);
    }
}