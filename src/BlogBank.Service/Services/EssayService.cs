using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Service.Interfaces;

namespace BlogBank.Service.Services;

public class EssayService(IEssayRepository repo) : IEssayService
{
    public Task<IEnumerable<Essay>> GetAllAsync() => repo.GetAllAsync();

    public Task<Essay?> GetByIdAsync(long id) => repo.GetByIdAsync(id);

    public Task<Essay> CreateAsync(Essay essay) => repo.CreateAsync(essay);

    public async Task<IEnumerable<Essay>> CreateBatchAsync(IEnumerable<Essay> essays)
        => await repo.CreateBatchAsync(essays);

    public Task<Essay?> UpdateAsync(long id, Essay essay) => repo.UpdateAsync(id, essay);

    public Task<bool> DeleteAsync(long id) => repo.DeleteAsync(id);
}
