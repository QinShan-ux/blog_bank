using BlogBank.Core.Entities;

namespace BlogBank.Core.Interfaces;

public interface IExportTaskRepository
{
    Task<long> AddTask();

    Task<ExportTask> GetTask(long id);
}