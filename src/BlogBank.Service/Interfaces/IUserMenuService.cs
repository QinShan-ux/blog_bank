using BlogBank.Core.Entities;

namespace BlogBank.Service.Interfaces;

public interface IUserMenuService
{
    Task<IEnumerable<UserMenu>> GetByUserIdAsync(long userId);
    Task<UserMenu> AssignAsync(long userId, long menuId);
    Task<int> BatchAssignAsync(long userId, IEnumerable<long> menuIds);
    Task ResetAsync(long userId, IEnumerable<long> menuIds);
    Task<bool> RevokeAsync(long userId, long menuId);
}
