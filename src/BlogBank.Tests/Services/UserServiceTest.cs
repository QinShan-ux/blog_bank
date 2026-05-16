using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Service.Services;
using JetBrains.Annotations;
using Moq;

namespace BlogBank.Tests.Services;

[TestSubject(typeof(UserService))]
public class UserServiceTest
{
    private readonly Mock<IUserRepository> _mockRepo;
    private readonly UserService _service;

    public UserServiceTest()
    {
        _mockRepo = new Mock<IUserRepository>();
        _service = new UserService(_mockRepo.Object); // 没有 DbContext
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnUsers()
    {
        // Arrange
        var fakeUsers = new List<User>
        {
            new User { Id = 1, Username = "Alice" },
            new User { Id = 2, Username = "Bob" }
        };
        _mockRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(fakeUsers);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }
}