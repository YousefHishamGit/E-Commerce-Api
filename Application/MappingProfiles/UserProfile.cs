using AutoMapper;
using Application.DTOs.Users;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Entity → DTO  (enums converted to their string names)
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Gender,
                           opt => opt.MapFrom(src => src.Gender.ToString()))
                .ForMember(dest => dest.Role,
                           opt => opt.MapFrom(src => src.Role.ToString()));

            // UpdateDto → Entity
            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

