using BlogBank.Core.Entities;

namespace BlogBank.Service.Interfaces;

public interface IEssayService
{
    Task<IEnumerable<Essay>> GetAllAsync();
    Task<Essay?> GetByIdAsync(long id);
    Task<Essay> CreateAsync(Essay essay);
    Task<IEnumerable<Essay>> CreateBatchAsync(IEnumerable<Essay> essays);
    Task<Essay?> UpdateAsync(long id, Essay essay);
    Task<bool> DeleteAsync(long id);
}
