using BlogBank.Infrastructure.Data;
using BlogBank.Infrastructure.Repositories;
using BlogBank.Core.Interfaces;
using Moq;

namespace BlogBank.Tests.Repositories;

public class ArticleRepositoryTests: IDisposable
{
    private readonly AppDbContext _db;
    private readonly Mock<ISnowflakeIdGenerator> _idGenMock;
    
    public void Dispose()
    {
        
    }
    
}