using AutoMapper;
using BlogBank.Core.Entities;
using BlogBank.Infrastructure.dtos;

namespace BlogBank.Api.proflie;

public class RoleProfile: Profile
{
    protected RoleProfile()
    {
        CreateMap<Role, RoleDto>()
            .ForMember(a => a.RoleName, 
                opt => opt.MapFrom(src => src.Name));
        
    }
}