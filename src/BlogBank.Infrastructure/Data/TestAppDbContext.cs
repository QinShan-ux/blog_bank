using BlogBank.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BlogBank.Infrastructure.Data;

public class TestAppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor, ISnowflakeIdGenerator idGen) : AppDbContext(options, httpContextAccessor, idGen)
{
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken); // 跳过审计，直接保存
    }
    protected override void SaveAudit() { }

    protected override void AutoFillAuditFields() { }
}