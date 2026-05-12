using System.Text.Json;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data;
using BlogBank.Infrastructure.dtos;
using Microsoft.EntityFrameworkCore;

namespace BlogBank.Api.Job;

public class TokenVersionCompensationJob(ICacheService cacheService,AppDbContext db,ILogger<TokenVersionCompensationJob> logger): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                // 取出待补偿的任务
                var item = await cacheService.ListLeftPopAsync("compensation:tokenVersion");
                if (string.IsNullOrEmpty(item))
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), ct);
                    continue;
                }

                var data = JsonSerializer.Deserialize<CompensationItem>(item);
                // await db.Users.Where(it => it.Id == data.UserId)
                //     .ExecuteUpdateAsync(s => 
                //     s.SetProperty(u => u.TokenVersion, data.version),ct);
                var user = await db.Users.FindAsync(data.UserId, ct);
                if (user!= null && user.TokenVersion < data.Version)
                {
                    user.TokenVersion = data.Version;
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("补偿成功 userId={UserId}", data.UserId);
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "补偿任务执行失败");
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
            }
        }
    }
}