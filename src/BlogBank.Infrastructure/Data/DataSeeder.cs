using BlogBank.Core.Entities;
using BlogBank.Core.Enums;
using BlogBank.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlogBank.Infrastructure.Data;

public class DataSeeder(
    AppDbContext db,
    ISnowflakeIdGenerator snowflake,
    IConfiguration config,
    ILogger<DataSeeder> logger,
    ICacheService cache)
{
    public async Task SeedAsync()
    {
        if (!bool.TryParse(config["Seed:Enabled"], out var enabled) || !enabled)
            return;

        var roleCode    = config["Seed:AdminRole:Code"]        ?? "admin";
        var roleName    = config["Seed:AdminRole:Name"]        ?? "管理员";
        var roleDesc    = config["Seed:AdminRole:Description"] ?? "系统管理员，拥有所有权限";
        var username    = config["Seed:SuperAdmin:Account"]   ?? "superadmin";
        var nickname    = config["Seed:SuperAdmin:Nickname"]   ?? "超级管理员";
        var email       = config["Seed:SuperAdmin:Email"]      ?? "superadmin@example.com";
        var password    = config["Seed:SuperAdmin:Password"]   ?? "Admin@123";
        var avatar      = config["Seed:SuperAdmin:Avatar"]     ?? string.Empty;

        // 1. 确保管理员角色存在
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Code == roleCode);
        if (role is null)
        {
            role = new Role { Code = roleCode, Name = roleName, Description = roleDesc };
            db.Roles.Add(role);
            await db.SaveChangesAsync();
            logger.LogInformation("Seed: 已创建角色 [{Code}] {Name}", roleCode, roleName);
        }

        // 2. 确保超级管理员用户存在
        var user = await db.Users.FirstOrDefaultAsync(u => u.Account == username);
        if (user is null)
        {
            user = new User
            {
                Id           = snowflake.NextId(),
                Account     = username,
                Nickname     = nickname,
                Email        = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Avatar       = avatar,
                IsEnabled    = true
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            logger.LogInformation("Seed: 已创建用户 [{Account}]", username);
        }

        // 3. 确保角色关联存在
        var exists = await db.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
        if (!exists)
        {
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
            await db.SaveChangesAsync();
            logger.LogInformation("Seed: 已为用户 [{Account}] 分配角色 [{Code}]", username, roleCode);
        }

        // 4. 确保菜单与用户菜单关联存在
        await SeedMenusAsync(user);
    }

    /// <summary>
    /// 菜单种子：菜单表为空时写入与前端内置菜单（DEFAULT_MENU_TREE）一致的默认菜单树，
    /// 再为超级管理员补齐全量菜单的用户菜单关联（前端按用户关联过滤菜单树，缺关联会整体回退到前端内置菜单）。
    /// 已有菜单数据时跳过写入，不覆盖菜单管理页的人工配置。
    /// </summary>
    private async Task SeedMenusAsync(User superAdmin)
    {
        if (!await db.Menus.AnyAsync())
        {
            // 结构与字段对齐 web/src/router/dynamicRoutes.ts 的 DEFAULT_MENU_TREE。
            // 注意：AppDbContext.AutoFillAuditFields 会在保存时为 Added 实体重赋雪花 Id，
            // 因此必须先保存父级拿到最终 Id，再创建引用其 Id 的子级（两段式保存）。
            var menuSeeds = new List<Menu>
            {
                NewMenu(0, MenuTypeEnum.Menu, "仪表盘",
                    name: "AdminDashboard", path: "dashboard", component: "dashboard/Index",
                    icon: "ele-Odometer", orderNo: 1, isKeepAlive: true, isAffix: true),
                NewMenu(0, MenuTypeEnum.Directory, "内容管理", icon: "ele-Notebook", orderNo: 2),
                NewMenu(0, MenuTypeEnum.Directory, "权限管理", icon: "ele-Lock", orderNo: 3),
                NewMenu(0, MenuTypeEnum.Directory, "系统工具", icon: "ele-SetUp", orderNo: 4),
                NewMenu(0, MenuTypeEnum.Directory, "工作", icon: "ele-Briefcase", orderNo: 5)
            };
            db.Menus.AddRange(menuSeeds);
            await db.SaveChangesAsync();

            var content    = menuSeeds[1];
            var permission = menuSeeds[2];
            var tools      = menuSeeds[3];
            var work       = menuSeeds[4];

            var childSeeds = new List<Menu>
            {
                NewMenu(content.Id, MenuTypeEnum.Menu, "文章管理",
                    name: "AdminArticles", path: "articles", component: "articles/Index",
                    icon: "ele-Document", orderNo: 1, isKeepAlive: true),
                NewMenu(content.Id, MenuTypeEnum.Menu, "随笔管理",
                    name: "AdminEssays", path: "essays", component: "essays/Index",
                    icon: "ele-EditPen", orderNo: 2, isKeepAlive: true),
                NewMenu(permission.Id, MenuTypeEnum.Menu, "用户管理",
                    name: "AdminUsers", path: "users", component: "users/Index",
                    icon: "ele-User", orderNo: 1, isKeepAlive: true),
                NewMenu(permission.Id, MenuTypeEnum.Menu, "角色管理",
                    name: "AdminRoles", path: "roles", component: "roles/Index",
                    icon: "ele-Avatar", orderNo: 2, isKeepAlive: true),
                NewMenu(permission.Id, MenuTypeEnum.Menu, "菜单管理",
                    name: "AdminMenus", path: "menus", component: "menus/Index",
                    icon: "ele-Menu", orderNo: 3, isKeepAlive: true),
                NewMenu(tools.Id, MenuTypeEnum.Menu, "审计日志",
                    name: "AdminAuditLogs", path: "audit-logs", component: "auditLogs/Index",
                    icon: "ele-List", orderNo: 1, isKeepAlive: true),
                NewMenu(tools.Id, MenuTypeEnum.Menu, "导出中心",
                    name: "AdminExport", path: "export", component: "export/Index",
                    icon: "ele-Download", orderNo: 2),
                NewMenu(work.Id, MenuTypeEnum.Menu, "Bug 记录",
                    name: "AdminWorkBugs", path: "work-bugs", component: "workBugs/Index",
                    icon: "ele-Monitor", orderNo: 1, isKeepAlive: true)
            };
            db.Menus.AddRange(childSeeds);
            await db.SaveChangesAsync();
            logger.LogInformation("Seed: 已写入默认菜单 {Count} 条", menuSeeds.Count + childSeeds.Count);
            // 失效菜单缓存：Redis 中的旧值（如空树）会跨重启存活，不清会让首个 TTL 周期内仍返回旧数据
            await cache.RemoveAsync("menus:all", "menus:tree");
        }
        else
        {
            logger.LogInformation("Seed: 菜单表已有数据，跳过菜单写入");
        }

        // 为超级管理员补齐全量关联：按复合键差集逐条补缺，不重复写入、不撤销已有授权
        var menuIds = await db.Menus.Select(m => m.Id).ToListAsync();
        var ownedMenuIds = await db.UserMenus
            .Where(um => um.UserId == superAdmin.Id)
            .Select(um => um.MenuId)
            .ToListAsync();
        var missingMenuIds = menuIds.Except(ownedMenuIds).ToList();
        if (missingMenuIds.Count > 0)
        {
            db.UserMenus.AddRange(missingMenuIds.Select(menuId => new UserMenu
            {
                UserId = superAdmin.Id,
                MenuId = menuId
            }));
            await db.SaveChangesAsync();
            logger.LogInformation("Seed: 已为用户 [{Account}] 补齐菜单关联 {Count} 条", superAdmin.Account, missingMenuIds.Count);
            await cache.RemoveAsync($"user-menus:user:{superAdmin.Id}");
        }
    }

    /// <summary>构造一条启用状态的种子菜单，缺省值与前端内置菜单的兜底工厂保持一致。</summary>
    private Menu NewMenu(long pid, MenuTypeEnum type, string title,
        string? name = null, string? path = null, string? component = null,
        string? icon = "ele-Menu", int orderNo = 100,
        bool isKeepAlive = false, bool isAffix = false) => new()
    {
        Id          = snowflake.NextId(),
        Pid         = pid,
        Type        = type,
        Name        = name,
        Path        = path,
        Component   = component,
        Title       = title,
        Icon        = icon,
        OrderNo     = orderNo,
        IsKeepAlive = isKeepAlive,
        IsAffix     = isAffix,
        Status      = MenuStatusEnum.Enabled
    };
}
