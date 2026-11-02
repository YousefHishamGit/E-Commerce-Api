using AutoMapper;
using Application.DTOs.Coupons;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class CouponProfile : Profile
    {
        public CouponProfile()
        {
            // Entity → DTO
            CreateMap<Coupon, CouponDto>()
                .ForMember(dest => dest.Type,
                           opt  => opt.MapFrom(src => src.Type.ToString()));

            // CreateDto → Entity
            CreateMap<CreateCouponDto, Coupon>()
                .ForMember(dest => dest.Type,
                           opt  => opt.MapFrom(src => Enum.Parse<Domain.Enums.CouponType>(src.Type)));

            // UpdateDto → Entity
            CreateMap<UpdateCouponDto, Coupon>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

