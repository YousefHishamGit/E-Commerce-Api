using AutoMapper;
using Application.DTOs.Offers;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class OfferProfile : Profile
    {
        public OfferProfile()
        {
            // Entity → DTO
            CreateMap<Offer, OfferDto>()
                .ForMember(dest => dest.ProductName,
                           opt  => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.CategoryName,
                           opt  => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

            // CreateDto → Entity
            CreateMap<CreateOfferDto, Offer>();

            // UpdateDto → Entity
            CreateMap<UpdateOfferDto, Offer>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

