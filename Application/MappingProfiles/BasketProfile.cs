using AutoMapper;
using Application.DTOs.Basket;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class BasketProfile : Profile
    {
        public BasketProfile()
        {
            // BasketItem → BasketItemDto (fields match 1-to-1)
            CreateMap<BasketItem, BasketItemDto>();

            // CustomerBasket → BasketDto
            CreateMap<CustomerBasket, BasketDto>()
                .ForMember(dest => dest.Items,
                           opt  => opt.MapFrom(src => src.Items));

            // BasketDto → CustomerBasket  (for update operations)
            CreateMap<BasketDto, CustomerBasket>()
                .ForMember(dest => dest.Items,
                           opt  => opt.MapFrom(src => src.Items));

            // BasketItemDto → BasketItem
            CreateMap<BasketItemDto, BasketItem>();
        }
    }
}

