using BlogBank.Core.Entities;

namespace BlogBank.Service.Interfaces;

public interface IMenuService
{
    Task<IEnumerable<Menu>> GetAllAsync();
    Task<Menu?> GetByIdAsync(long id);
    Task<bool> HasChildrenAsync(long id);
    Task<Menu> CreateAsync(Menu menu);
    Task<Menu?> UpdateAsync(long id, Menu menu);
    Task<bool> DeleteAsync(long id);
}
