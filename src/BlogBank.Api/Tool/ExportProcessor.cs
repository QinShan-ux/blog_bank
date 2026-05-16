using System.Linq.Expressions;
using BlogBank.Api.Models;
using BlogBank.Core.Entities;
using BlogBank.Core.Enums;
using BlogBank.Infrastructure.Data;
using BlogBank.Infrastructure.Data.Configurations;
using BlogBank.Infrastructure.Entities;
using BlogBank.Service.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;

namespace BlogBank.Api.Tool;

public class ExportProcessor(
    AppDbContext db,
    IStorageService storage,
    IHubContext<ExportHub> hubClients,
    ILogger<ExportProcessor> logger)
{
    public async Task ProcessAsync(ExportJob job)
    {
        var task = await db.ExportTasks.FindAsync(job.TaskId);
        if (task is null)
        {
            logger.LogError("导出任务不存在 | TaskId={TaskId}", job.TaskId);
            return;
        }

        task.Status = TaskEnum.Processing;
        await db.SaveChangesAsync();

        // 临时文件放在系统临时目录，避免目录不存在问题
        var exportDir = Path.Combine(Path.GetTempPath(), "exports");
        Directory.CreateDirectory(exportDir);  // 不存在则创建，已存在不报错

        var filePath = Path.Combine(exportDir, $"{job.TaskId}.xlsx");

        try
        {
            var total = await db.Articles
                .Where(BuildFilter(job.QueryRequest))
                .CountAsync();

            logger.LogInformation("开始导出 | TaskId={TaskId} Total={Total}", job.TaskId, total);

            await MiniExcel.SaveAsAsync(filePath, StreamData(job.QueryRequest));

            var fileUrl = await storage.UploadAsync(filePath);

            task.Status      = TaskEnum.Done;
            task.Progress    = 100;
            task.FileUrl     = fileUrl;
            task.CompletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            logger.LogInformation("导出完成 | TaskId={TaskId} FileUrl={FileUrl}", job.TaskId, fileUrl);

            await hubClients.Clients
                .User(task.OperatorId)
                .SendAsync("ExportDone", new { fileUrl });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "导出失败 | TaskId={TaskId}", job.TaskId);
            task.Status = TaskEnum.Failed;
            await db.SaveChangesAsync();
            throw;
        }
        finally
        {
            // 无论成功失败都清理临时文件
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    private async IAsyncEnumerable<Article> StreamData(ArticleQueryRequest query)
    {
        var page     = 0;
        var pageSize = 1000;

        while (true)
        {
            var batch = await db.Articles
                .Where(BuildFilter(query))
                .OrderBy(o => o.Id)
                .Skip(page * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            if (!batch.Any()) yield break;

            foreach (var row in batch)
                yield return row;

            page++;
        }
    }

    private Expression<Func<Article, bool>> BuildFilter(ArticleQueryRequest query)
    {
        var predicate = PredicateBuilder.New<Article>(true);

        if (query.StartTime.HasValue)
            predicate = predicate.And(o => o.CreatedAt >= query.StartTime.Value);

        if (query.EndTime.HasValue)
            predicate = predicate.And(o => o.CreatedAt <= query.EndTime.Value);

        if (!query.IsDelete)
            predicate = predicate.And(o => o.IsDeleted == query.IsDelete);

        return predicate;
    }
}