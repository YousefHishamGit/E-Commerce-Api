using AutoMapper;
using Application.DTOs.Wishlist;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class WishlistProfile : Profile
    {
        public WishlistProfile()
        {
            // WishlistItem → WishlistItemDto
            CreateMap<WishlistItem, WishlistItemDto>()
                .ForMember(dest => dest.ProductName,
                           opt  => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductPrice,
                           opt  => opt.MapFrom(src => src.Product != null ? src.Product.Price : 0))
                .ForMember(dest => dest.ProductPhoto,
                           opt  => opt.MapFrom(src => src.Product != null ? src.Product.Photo : null))
                .ForMember(dest => dest.StockQuantity,
                           opt  => opt.MapFrom(src => src.Product != null ? src.Product.StockQuantity : 0));

            // Wishlist → WishlistDto
            CreateMap<Wishlist, WishlistDto>()
                .ForMember(dest => dest.Items,
                           opt  => opt.MapFrom(src => src.Items));
        }
    }
}

