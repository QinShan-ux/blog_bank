using BlogBank.Core.Entities;

namespace BlogBank.Service.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(long id);
    Task<bool> CodeExistsAsync(string code, long? excludeId = null);
    Task<Role> CreateAsync(Role role);
    Task<Role?> UpdateAsync(long id, Role role);
    Task<bool> DeleteAsync(long id);
}
