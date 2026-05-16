using BlogBank.Core.Entities;
using BlogBank.Core.Enums;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace BlogBank.Infrastructure.Repositories;

public class ExportTaskRepository(AppDbContext db,ISnowflakeIdGenerator sno,IHttpContextAccessor contextAccessor): IExportTaskRepository
{
    public async Task<long> AddTask()
    {
        var operatorId = contextAccessor.HttpContext?
            .User.FindFirst("UserId")?.Value;
        var id = sno.NextId();;
        var task = new ExportTask()
        {
            Id = id,
            Status = TaskEnum.Pending,
            FileUrl = $"export_{id}.xlsx",
            ErrorMessage = "",
            OperatorId = operatorId,
        };
        db.ExportTasks.Add(task);
        await db.SaveChangesAsync();
        return task.Id;
    }

    public Task<ExportTask> GetTask(long id)
    {
        return db.ExportTasks.FindAsync(id).AsTask();
    }
}