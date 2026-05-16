using System.Text;
using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data;
using BlogBank.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BlogBank.Tests.Repositories;

public class ArticleRepositoryTest
{
    private AppDbContext _dbContext;
    private ArticleRepository _repository;

    public ArticleRepositoryTest()
    {
        // 1. 配置 In-Memory 数据库
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // 2. Mock IHttpContextAccessor
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(new DefaultHttpContext());

        // 3. Mock ISnowflakeIdGenerator
        var idGen = new Mock<ISnowflakeIdGenerator>();
        idGen
            .Setup(x => x.NextId())
            .Returns(1233L); // 返回固定 ID，方便断言

        // 4. 创建 DbContext 实例
        _dbContext = new AppDbContext(options, httpContextAccessor.Object, idGen.Object);

        // 5. 预置测试数据
        _dbContext.Articles.Add(new Article 
            { Id = 1233, Title = "Test Article",
                Date = DateOnly.FromDateTime(DateTime.Now),
                Category = "cs",Content = "hello world",
                CreatedAt = DateTime.Now,Excerpt = "hello",
                IsDeleted = false,
                CreatedBy = "cs",
                UpdatedBy = "",
                UpdatedAt = DateTime.Now,
                ReadTime = "100",
                RowVersion = Encoding.UTF8.GetBytes("string")
            });
        _dbContext.SaveChanges();

        _repository = new ArticleRepository(_dbContext,idGen.Object);
    }

    [Theory]
    [InlineData(12334)]
    public async Task GetById_ShouldReturnArticle_WhenArticleExists(long id)
    {
        // Arrange
        _dbContext.Articles.Add(new Article
        {
            Id = 12334, Title = "Test Article",
            Date = DateOnly.FromDateTime(DateTime.Now),
            Category = "cs", Content = "hello world",
            CreatedAt = DateTime.Now, Excerpt = "hello",
            IsDeleted = false,
            CreatedBy = "cs",
            UpdatedBy = "",
            UpdatedAt = DateTime.Now,
            ReadTime = "100",
            RowVersion = Encoding.UTF8.GetBytes("string")
        });
        // Act
        var result = await _repository.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }
}