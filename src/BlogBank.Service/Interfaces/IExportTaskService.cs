using BlogBank.Core.Entities;

namespace BlogBank.Service.Interfaces;

public interface IExportTaskService
{
    Task<long> AddTask();

    Task<ExportTask> GetTask(long id);
}