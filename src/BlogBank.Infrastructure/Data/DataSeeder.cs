using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlogBank.Infrastructure.Data;

public class DataSeeder(
    AppDbContext db,
    ISnowflakeIdGenerator snowflake,
    IConfiguration config,
    ILogger<DataSeeder> logger)
{
    public async Task SeedAsync()
    {
        if (!bool.TryParse(config["Seed:Enabled"], out var enabled) || !enabled)
            return;

        var roleCode    = config["Seed:AdminRole:Code"]        ?? "admin";
        var roleName    = config["Seed:AdminRole:Name"]        ?? "管理员";
        var roleDesc    = config["Seed:AdminRole:Description"] ?? "系统管理员，拥有所有权限";
        var username    = config["Seed:SuperAdmin:Username"]   ?? "superadmin";
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
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null)
        {
            user = new User
            {
                Id           = snowflake.NextId(),
                Username     = username,
                Nickname     = nickname,
                Email        = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Avatar       = avatar,
                IsEnabled    = true
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            logger.LogInformation("Seed: 已创建用户 [{Username}]", username);
        }

        // 3. 确保角色关联存在
        var exists = await db.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
        if (!exists)
        {
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
            await db.SaveChangesAsync();
            logger.LogInformation("Seed: 已为用户 [{Username}] 分配角色 [{Code}]", username, roleCode);
        }
    }
}
