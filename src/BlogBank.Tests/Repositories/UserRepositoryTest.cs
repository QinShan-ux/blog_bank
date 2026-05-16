using System.Text;
using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data;
using BlogBank.Infrastructure.Repositories;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BlogBank.Tests.Repositories;

[TestSubject(typeof(UserRepository))]
public class UserRepositoryTest
{
    private readonly AppDbContext _dbContext;
    private readonly UserRepository _repository;

    public UserRepositoryTest()
    {
        // 1. InMemory 数据库
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // 2. Mock 需要的依赖
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext)
                           .Returns(new DefaultHttpContext());

        var idGen = new Mock<ISnowflakeIdGenerator>();
        var count = 1L;
        idGen.Setup(x => x.NextId()).Returns(() => count++);

        var cacheService = new Mock<ICacheService>(); // 当前方法没用到，但构造函数需要

        // 3. ILogger 直接用 NullLogger，不需要 Mock
        var logger = NullLogger<UserRepository>.Instance;

        // 4. 创建实例
        _dbContext = new TestAppDbContext(options, httpContextAccessor.Object, idGen.Object);
        _repository = new UserRepository(_dbContext, idGen.Object, logger, cacheService.Object);
    }

    // ✅ 测试 GetAllAsync：按创建时间倒序
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers_OrderedByCreatedAtDesc()
    {
        // Arrange
        _dbContext.Users.AddRange(
            CreateTestUser(1,"Alice", new DateTime(2024, 1, 1)),
            CreateTestUser(2,"Bob", new DateTime(2024, 2, 1))
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Bob",   result[0].Username); // 最新的在前
        Assert.Equal("Alice",   result[1].Username); // 最新的在前
    }

    // ✅ 测试 GetByIdAsync：找到用户，并预加载角色
    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WithRoles_WhenExists()
    {
        // Arrange
        var role = new Role { Id = 10, Name = "Admin",CreatedBy = "测试",RowVersion = Encoding.UTF8.GetBytes("hello"),UpdatedBy="测试"};
        var user = CreateTestUser(1,"Alice", DateTime.UtcNow);
        user.UserRoles = new List<UserRole>
        {
            new UserRole { UserId = 1, RoleId = 10, Role = role }
        };

        _dbContext.Roles.Add(role);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        
        var all = await _dbContext.Users.ToListAsync();
        var allRole = await _dbContext.Roles.ToListAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Alice", result.Username);
        Assert.Single(result.UserRoles);                        // 有一个角色
        Assert.Equal("Admin", result.UserRoles[0].Role.Name);  // 角色名正确
    }

    // ✅ 测试 GetByIdAsync：用户不存在返回 null
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(9999);

        // Assert
        Assert.Null(result);
    }

    // ✅ 测试 GetAllAsync：没有数据时返回空集合
    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoUsers()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    // 工厂方法
    private static User CreateTestUser(long id, string name, DateTime createdAt)
    {
        return new User
        {
            Id = id,
            Username = name,
            CreatedAt = createdAt,
            UpdatedBy = "测试",
            CreatedBy = "测试",
            // 其他必填字段...
            RowVersion = Encoding.UTF8.GetBytes("hello")
        };
    }
}