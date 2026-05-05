using System.Linq.Expressions;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using BlogBank.Core.dto;
using BlogBank.Core.Entities;
using BlogBank.Core.Enums;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data.filter;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.IdentityModel.Tokens;

namespace BlogBank.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor,ISnowflakeIdGenerator idGen)
    : DbContext(options)
{
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleTag> ArticleTags => Set<ArticleTag>();
    public DbSet<Essay> Essays => Set<Essay>();
    public DbSet<EssayTag> EssayTags => Set<EssayTag>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<UserMenu> UserMenus => Set<UserMenu>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        #region 基础类配置

        // 基础类配置（软删除过滤）
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType, builder =>
            {
                builder.Property("Id")
                    .ValueGeneratedNever()
                    .HasComment("主键");
                builder.Property("CreatedAt")
                    .IsRequired()
                    .HasColumnType("datetime(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .HasComment("创建时间");

                builder.Property("UpdatedAt")
                    .IsRequired()
                    .HasColumnType("datetime(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .HasComment("修改时间");

                // 软删除全局过滤
                builder.HasQueryFilter(
                    BuildIsDeletedFilter(entityType.ClrType)
                );
            });
        }

        #endregion
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    // 动态建立 Lambda：e => !e.IsDeleted
    private static LambdaExpression BuildIsDeletedFilter(Type type)
    {
        var param = Expression.Parameter(type, "e");
        var body = Expression.Not(
            Expression.Property(param, nameof(BaseEntity.IsDeleted))
        );
        return Expression.Lambda(body, param);
    }

    // 使用EFCore拦截器，
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new AuditInterceptor());
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        // 1. 自动填充 CreatedAt / UpdatedAt / CreatedBy / UpdatedBy
        AutoFillAuditFields();

        // 2. 在保存前捕获变更（必须在 SaveChanges 之前！此时 OriginalValue 还在）
        var auditEntries = GetAuditEntries();

        // 3. 保存业务数据
        var result = await base.SaveChangesAsync(cancellationToken);

        // 4. 保存审计日志（业务数据保存成功后再保存日志）
        if (auditEntries.Any())
        {
            AuditLogs.AddRange(auditEntries);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    // 自动填充时间和操作人
    private void AutoFillAuditFields()
    {
        var httpInfo = GetHttpInfo();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.Now;
                    entry.Entity.CreatedBy = httpInfo?.UserName;
                    entry.Entity.Id = idGen.NextId();
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.Now;
                    entry.Entity.UpdatedBy = httpInfo?.UserName;
                    // 防止意外修改创建时间
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;
                    break;
            }
        }
    }

    // 捕获所有变更，生成审计日志
    private List<AuditLog> GetAuditEntries()
    {
        // 从 HttpContext 取 ActionFilter 存入的 HTTP 层信息
        var httpInfo = GetHttpInfo();

        return ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added
                        || e.State == EntityState.Modified
                        || e.State == EntityState.Deleted)
            .Select(e => new AuditLog
            {
                Id = idGen.NextId(),
                // HTTP 层信息（来自 ActionFilter）
                TraceId = httpInfo.TraceId,
                UserId = httpInfo.UserId,
                UserName = httpInfo.UserName,
                RequestUrl = httpInfo.RequestUrl,
                IpAddress = httpInfo.IpAddress,
                HttpMethod = httpInfo.HttpMethod,
                OperatedAt = httpInfo?.OperatedAt ?? DateTime.Now,

                // 数据层信息（ChangeTracker 天然知道）
                TableName = e.Entity.GetType().Name,
                EntityId = e.Entity.Id.ToString(),
                Action = e.State switch
                {
                    EntityState.Added => "新增",
                    EntityState.Modified => "修改",
                    EntityState.Deleted => "删除",
                    _ => "未知"
                },

                // 旧值：只记录发生变化的字段，新增没有旧值
                OldValues = e.State == EntityState.Added
                    ? ""
                    : JsonSerializer.Serialize(
                        e.Properties
                            .Where(p => p.IsModified)
                            .ToDictionary(
                                p => p.Metadata.Name,
                                p => p.OriginalValue)), // ← 修改前的值

                // 新值：只记录发生变化的字段，删除没有新值
                NewValues = GetNewValues(e) // ← 修改后的值
            })
            .ToList();
    }

    // 从 HttpContext 取 ActionFilter 存入的信息
    private AuditHttpInfo GetHttpInfo()
    {
        return httpContextAccessor.HttpContext?
            .Items["AuditInfo"] as AuditHttpInfo ;
    }

    private string GetNewValues(EntityEntry<BaseEntity> e)
    {
        switch (e.State)
        {
            case EntityState.Added:
                return JsonSerializer.Serialize(e.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue),JsonOptions);
            case EntityState.Modified:
                return JsonSerializer.Serialize(
                    e.Properties
                        .Where(p => p.IsModified)
                        .ToDictionary(
                            p => p.Metadata.Name,
                            p => p.CurrentValue)
                    ,JsonOptions);
        }

        return "{}";
    }
}