using AutoMapper;
using Application.DTOs.Reviews;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class ReviewProfile : Profile
    {
        public ReviewProfile()
        {
            // Entity → DTO
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.UserFullName,
                           opt  => opt.MapFrom(src => src.User != null
                               ? $"{src.User.FirstName} {src.User.LastName}"
                               : string.Empty))
                .ForMember(dest => dest.UserId,
                           opt  => opt.MapFrom(src => src.User != null ? src.User.Id : Guid.Empty));

            // CreateDto → Entity  (UserId set manually in service from JWT claims)
            CreateMap<CreateReviewDto, Review>();

            // UpdateDto → Entity
            CreateMap<UpdateReviewDto, Review>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

