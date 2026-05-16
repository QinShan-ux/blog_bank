using BlogBank.Core.Entities;

namespace BlogBank.Service.Interfaces;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(long id);
    Task<bool> UsernameExistsAsync(string username, long? excludeId = null);
    Task<bool> EmailExistsAsync(string email, long? excludeId = null);
    Task<User> CreateAsync(User user);
    Task<User?> UpdateAsync(long id, User user);
    Task<bool> DeleteAsync(long id);
}
