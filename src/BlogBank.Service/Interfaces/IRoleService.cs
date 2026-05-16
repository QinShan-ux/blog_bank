using BlogBank.Core.Entities;

namespace BlogBank.Service.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int id);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);
    Task<Role> CreateAsync(Role role);
    Task<Role?> UpdateAsync(int id, Role role);
    Task<bool> DeleteAsync(int id);
}
