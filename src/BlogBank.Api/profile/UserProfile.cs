using AutoMapper;
using BlogBank.Api.Dtos;
using BlogBank.Core.Entities;

namespace BlogBank.Api.profile;

public class UserProfile: Profile
{
    public UserProfile()
    {
        
        CreateMap<User, UserTestDto>()
            .ForMember(a => a.youxiang,opt => opt.MapFrom(src => src.Email))
            .ForMember(a => a.dianhuan,opt => opt.MapFrom(src => src.Phone));
    }
}