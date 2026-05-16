using BlogBank.Core.Entities;

namespace BlogBank.Service.Interfaces;

public interface IUserRoleService
{
    Task<IEnumerable<UserRole>> GetByUserIdAsync(long userId);
    Task<IEnumerable<UserRole>> GetByRoleIdAsync(int roleId);
    Task<UserRole> AssignAsync(long userId, int roleId);
    Task<bool> RevokeAsync(long userId, int roleId);
}
