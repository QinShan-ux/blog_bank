using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Service.Interfaces;

namespace BlogBank.Service.Services;

public class UserService(IUserRepository repo) : IUserService
{
    public Task<IEnumerable<User>> GetAllAsync() => repo.GetAllAsync();

    public Task<User?> GetByIdAsync(long id) => repo.GetByIdAsync(id);

    public Task<bool> UsernameExistsAsync(string username, long? excludeId = null)
        => repo.UsernameExistsAsync(username, excludeId);

    public Task<bool> EmailExistsAsync(string email, long? excludeId = null)
        => repo.EmailExistsAsync(email, excludeId);

    public Task<User> CreateAsync(User user) => repo.CreateAsync(user);

    public Task<User?> UpdateAsync(long id, User user) => repo.UpdateAsync(id, user);

    public Task<bool> DeleteAsync(long id) => repo.DeleteAsync(id);
}
