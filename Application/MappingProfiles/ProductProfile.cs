using AutoMapper;
using Application.DTOs.Products;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            // Entity → DTO
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName,
                           opt  => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));

            // CreateDto → Entity
            CreateMap<CreateProductDto, Product>();

            // UpdateDto → Entity  (null values are ignored — only provided fields are updated)
            CreateMap<UpdateProductDto, Product>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

