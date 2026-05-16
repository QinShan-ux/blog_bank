using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Service.Interfaces;

namespace BlogBank.Service.Services;

public class MenuService(IMenuRepository repo) : IMenuService
{
    public Task<IEnumerable<Menu>> GetAllAsync() => repo.GetAllAsync();

    public Task<Menu?> GetByIdAsync(long id) => repo.GetByIdAsync(id);

    public Task<bool> HasChildrenAsync(long id) => repo.HasChildrenAsync(id);

    public Task<Menu> CreateAsync(Menu menu) => repo.CreateAsync(menu);

    public Task<Menu?> UpdateAsync(long id, Menu menu) => repo.UpdateAsync(id, menu);

    public Task<bool> DeleteAsync(long id) => repo.DeleteAsync(id);
}
