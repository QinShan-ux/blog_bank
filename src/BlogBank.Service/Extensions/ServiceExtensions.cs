using BlogBank.Service.Interfaces;
using BlogBank.Service.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlogBank.Service.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IEssayService, EssayService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IUserMenuService, UserMenuService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}