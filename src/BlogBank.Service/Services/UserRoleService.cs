using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Service.Interfaces;

namespace BlogBank.Service.Services;

public class UserRoleService(IUserRoleRepository repo) : IUserRoleService
{
    public Task<IEnumerable<UserRole>> GetByUserIdAsync(long userId) => repo.GetByUserIdAsync(userId);

    public Task<IEnumerable<UserRole>> GetByRoleIdAsync(int roleId) => repo.GetByRoleIdAsync(roleId);

    public Task<UserRole> AssignAsync(long userId, int roleId) => repo.AssignAsync(userId, roleId);

    public Task<bool> RevokeAsync(long userId, int roleId) => repo.RevokeAsync(userId, roleId);
}
