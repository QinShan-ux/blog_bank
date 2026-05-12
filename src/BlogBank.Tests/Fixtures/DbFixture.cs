using BlogBank.Infrastructure.Data;
using BlogBank.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BlogBank.Tests.Fixtures;

// BlogBank.Tests/Fixtures/DbFixture.cs
public class DbFixture : IAsyncLifetime
{
    public AppDbContext Db { get; private set; } = null!;
    
    // 暴露给测试类复用，避免每个测试自己 new
    public FakeSnowflakeIdGenerator IdGen { get; } = new();

    public async Task InitializeAsync()
    {
        var connectionString = BuildConnectionString();
        var serverVersion = ServerVersion.AutoDetect(connectionString);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(connectionString, serverVersion,
                mySqlOptions => mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null))
            .Options;

        Db = new AppDbContext(
            options,
            new FakeHttpContextAccessor(),
            IdGen
        );

        await Db.Database.EnsureDeletedAsync();
        await Db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await Db.DisposeAsync();
    }

    private static string BuildConnectionString()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        return config.GetConnectionString("Default")
               ?? throw new InvalidOperationException(
                   "未找到连接字符串，请配置 appsettings.Test.json 或环境变量 ConnectionStrings__Default");
    }
}