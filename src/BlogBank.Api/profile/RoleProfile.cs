using AutoMapper;
using BlogBank.Core.Entities;
using BlogBank.Infrastructure.dtos;

namespace BlogBank.Api.profile;

public class RoleProfile: Profile
{
    public RoleProfile()
    {
        CreateMap<Role, RoleDto>()
            .ForMember(a => a.RoleName, 
                opt => opt.MapFrom(src => src.Name))
            .ForMember(a => a.RoleCode,
                opt => opt.MapFrom(src => src.Code));
        
        CreateMap<RoleDto, Role>()
            .ForAllMembers(opt =>
            {
                opt.MapFrom(src => src.RoleName);
                opt.MapFrom(src => src.RoleCode);
            });
        
    }
}