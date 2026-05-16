using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Service.Interfaces;

namespace BlogBank.Service.Services;

public class UserMenuService(IUserMenuRepository repo) : IUserMenuService
{
    public Task<IEnumerable<UserMenu>> GetByUserIdAsync(long userId) => repo.GetByUserIdAsync(userId);

    public Task<UserMenu> AssignAsync(long userId, long menuId) => repo.AssignAsync(userId, menuId);

    public Task<int> BatchAssignAsync(long userId, IEnumerable<long> menuIds) => repo.BatchAssignAsync(userId, menuIds);

    public Task ResetAsync(long userId, IEnumerable<long> menuIds) => repo.ResetAsync(userId, menuIds);

    public Task<bool> RevokeAsync(long userId, long menuId) => repo.RevokeAsync(userId, menuId);
}
