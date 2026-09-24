using BlogBank.Core.Entities;
using BlogBank.Core.Enums;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data;
using BlogBank.Tests.Fakes;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BlogBank.Tests.Services;

[TestSubject(typeof(DataSeeder))]
public class DataSeederTest
{
    private readonly AppDbContext _db;
    private readonly DataSeeder _seeder;
    private readonly Mock<ICacheService> _cache = new();

    public DataSeederTest()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        // 与生产一致：DbContext 与 DataSeeder 共享同一雪花生成器单例（AutoFill 会用它重赋 Added 实体 Id）
        var idGen = new FakeSnowflakeIdGenerator();
        _db = new SeederTestDbContext(options, new FakeHttpContextAccessor(), idGen);
        _seeder = new DataSeeder(_db, idGen, BuildConfig(enabled: true), NullLogger<DataSeeder>.Instance, _cache.Object);
    }

    /// <summary>
    /// 测试专用上下文：保留生产审计字段填充（含 Added 实体雪花 Id 重赋，与线上一致），
    /// 仅关闭审计日志写入，并为 InMemory 补齐必需的 RowVersion 值。
    /// </summary>
    private class SeederTestDbContext(
        DbContextOptions<AppDbContext> options,
        IHttpContextAccessor httpContextAccessor,
        ISnowflakeIdGenerator idGen) : AppDbContext(options, httpContextAccessor, idGen)
    {
        protected override void SaveAudit() { }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>().Where(e => e.State == EntityState.Added))
                entry.Entity.RowVersion = Array.Empty<byte>();
            return base.SaveChangesAsync(cancellationToken);
        }
    }

    // ✅ 空库首次种子：写入 13 条默认菜单，superadmin 关联全部菜单
    [Fact]
    public async Task SeedAsync_OnEmptyDatabase_ShouldSeed13MenusAndLinkAllToSuperAdmin()
    {
        await _seeder.SeedAsync();

        var menus = await _db.Menus.ToListAsync();
        Assert.Equal(13, menus.Count);

        var superAdmin = await _db.Users.SingleAsync(u => u.Account == "superadmin");
        var links = await _db.UserMenus.Where(um => um.UserId == superAdmin.Id).ToListAsync();
        Assert.Equal(13, links.Count);

        // 结构抽查：仪表盘为根级页面、固定在标签栏、启用状态
        var dashboard = menus.Single(m => m.Name == "AdminDashboard");
        Assert.Equal(0L, dashboard.Pid);
        Assert.Equal(MenuTypeEnum.Menu, dashboard.Type);
        Assert.Equal("dashboard/Index", dashboard.Component);
        Assert.True(dashboard.IsAffix);
        Assert.Equal(MenuStatusEnum.Enabled, dashboard.Status);

        // 「Bug 记录」挂在「工作」目录下
        var work = menus.Single(m => m.Type == MenuTypeEnum.Directory && m.Title == "工作");
        var workBugs = menus.Single(m => m.Name == "AdminWorkBugs");
        Assert.Equal(work.Id, workBugs.Pid);

        // 雪花 ID 唯一
        Assert.Equal(13, menus.Select(m => m.Id).Distinct().Count());

        // 种子写入后失效菜单与用户关联缓存
        _cache.Verify(c => c.RemoveAsync("menus:all", "menus:tree"), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    // ✅ 重复种子：菜单与关联均不重复写入
    [Fact]
    public async Task SeedAsync_SecondRun_ShouldNotDuplicateMenusOrLinks()
    {
        await _seeder.SeedAsync();
        await _seeder.SeedAsync();

        Assert.Equal(13, await _db.Menus.CountAsync());
        var superAdmin = await _db.Users.SingleAsync(u => u.Account == "superadmin");
        Assert.Equal(13, await _db.UserMenus.CountAsync(um => um.UserId == superAdmin.Id));
    }

    // ✅ 已有菜单数据时重启：不追加、不修改菜单，仅为 superadmin 按当前菜单补缺关联
    [Fact]
    public async Task SeedAsync_WithExistingMenus_ShouldKeepDataAndOnlyFillMissingLinks()
    {
        // 预置一条人工菜单（模拟菜单管理页维护的数据），无任何用户关联
        var custom = new Menu { Id = 1, Pid = 0, Type = MenuTypeEnum.Menu, Title = "自定义菜单", Path = "custom", Component = "custom/Index" };
        _db.Menus.Add(custom);
        await _db.SaveChangesAsync();

        await _seeder.SeedAsync();

        var menus = await _db.Menus.ToListAsync();
        Assert.Single(menus);
        var row = menus.Single(m => m.Id == custom.Id);
        Assert.Equal("自定义菜单", row.Title);

        // 关联按当前全部菜单补缺：superadmin 与自定义菜单建立关联
        var superAdmin = await _db.Users.SingleAsync(u => u.Account == "superadmin");
        var links = await _db.UserMenus.Where(um => um.UserId == superAdmin.Id).ToListAsync();
        Assert.Single(links);
        Assert.Equal(custom.Id, links[0].MenuId);
    }

    // ✅ 人工撤销部分关联后重启：仅补回缺失关联，菜单不变
    [Fact]
    public async Task SeedAsync_AfterLinksRevoked_ShouldRefillMissingLinksOnly()
    {
        await _seeder.SeedAsync();

        var superAdmin = await _db.Users.SingleAsync(u => u.Account == "superadmin");
        var removed = await _db.UserMenus.Where(um => um.UserId == superAdmin.Id).Take(5).ToListAsync();
        _db.UserMenus.RemoveRange(removed);
        await _db.SaveChangesAsync();
        Assert.Equal(8, await _db.UserMenus.CountAsync(um => um.UserId == superAdmin.Id));

        await _seeder.SeedAsync();

        Assert.Equal(13, await _db.Menus.CountAsync());
        Assert.Equal(13, await _db.UserMenus.CountAsync(um => um.UserId == superAdmin.Id));
    }

    // ✅ 种子开关关闭：不写任何数据
    [Fact]
    public async Task SeedAsync_WhenSeedDisabled_ShouldWriteNothing()
    {
        var seeder = new DataSeeder(_db, new FakeSnowflakeIdGenerator(), BuildConfig(enabled: false), NullLogger<DataSeeder>.Instance, _cache.Object);
        await seeder.SeedAsync();

        Assert.Empty(await _db.Menus.ToListAsync());
        Assert.Empty(await _db.UserMenus.ToListAsync());
        Assert.Empty(await _db.Users.ToListAsync());
        _cache.VerifyNoOtherCalls();
    }

    private static IConfiguration BuildConfig(bool enabled) => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Seed:Enabled"] = enabled ? "true" : "false"
        })
        .Build();
}
