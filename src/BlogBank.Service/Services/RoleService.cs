using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Service.Interfaces;

namespace BlogBank.Service.Services;

public class RoleService(IRoleRepository repo) : IRoleService
{
    public Task<IEnumerable<Role>> GetAllAsync() => repo.GetAllAsync();

    public Task<Role?> GetByIdAsync(int id) => repo.GetByIdAsync(id);

    public Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        => repo.CodeExistsAsync(code, excludeId);

    public Task<Role> CreateAsync(Role role) => repo.CreateAsync(role);

    public Task<Role?> UpdateAsync(int id, Role role) => repo.UpdateAsync(id, role);

    public Task<bool> DeleteAsync(int id) => repo.DeleteAsync(id);
}
