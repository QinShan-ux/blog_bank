using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Common;
using BlogBank.Infrastructure.Data;
using BlogBank.Infrastructure.Repositories;
using BlogBank.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BlogBank.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// 注册基础设施层服务。
    /// 通过 appsettings.json 中的 DatabaseProvider 配置选择数据库（mysql / postgresql）。
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = (configuration["DatabaseProvider"] ?? "postgresql").ToLower();

        // 数据库注册
        services.AddDbContext<AppDbContext>(options =>
        {
            switch (provider)
            {
                case "mysql":
                    var mysqlConn = configuration.GetConnectionString("MySQL")
                        ?? throw new InvalidOperationException("ConnectionStrings:MySQL is not configured.");
                    options.UseMySql(mysqlConn, ServerVersion.AutoDetect(mysqlConn));
                    break;

                case "postgresql":
                    var pgConn = configuration.GetConnectionString("PostgreSQL")
                        ?? throw new InvalidOperationException("ConnectionStrings:PostgreSQL is not configured.");
                    options.UseNpgsql(pgConn);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported DatabaseProvider: '{provider}'. Valid values are 'mysql' or 'postgresql'.");
            }

            options.LogTo(Console.WriteLine, LogLevel.Information) //输出所有sql
                .EnableSensitiveDataLogging(); //显示参数
        });
        services.AddHttpContextAccessor();

        var machineId = long.TryParse(configuration["Snowflake:MachineId"], out var mid) ? mid : 1L;
        services.AddSingleton<ISnowflakeIdGenerator>(_ => new SnowflakeIdGenerator(machineId));

        var redisEnabled = configuration["Redis:Enabled"] != "false";
        if (redisEnabled)
        {
            var redisConn = configuration["Redis:ConnectionString"] ?? "localhost:6379";
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var opts = ConfigurationOptions.Parse(redisConn);
                opts.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(opts);
            });
        }

        services.AddMemoryCache();
        services.AddScoped<ICacheService>(sp => new CacheService(
            sp.GetService<IConnectionMultiplexer>(),
            sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
            configuration));
        services.AddScoped<ITokenService>(sp => new TokenService(
            configuration,
            sp.GetService<IConnectionMultiplexer>(),
            sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>()));

        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IEssayRepository, EssayRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IUserMenuRepository, UserMenuRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<DataSeeder>();

        return services;
    }
}
