using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Infrastructure.Entities;
using BlogBank.Infrastructure.Services;
using BlogBank.Service.Interfaces;

namespace BlogBank.Service.Services;

public class ExportTaskService(IExportTaskRepository repository,IMessagePublisher publisher): IExportTaskService
{
    public async Task<long> AddTask()
    {
        var res = await repository.AddTask();
        var job = new ExportJob()
        {
            TaskId = res,
            QueryRequest = new ArticleQueryRequest()
            {
                IsDelete = false
            }
        };
        await publisher.PublishAsync("export.exchange","export.job",job);
        return res;
    }

    public Task<ExportTask> GetTask(long id)
    {
        return repository.GetTask(id);
    }
}