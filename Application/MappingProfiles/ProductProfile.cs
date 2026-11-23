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
                           opt  => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.Images,
                           opt  => opt.MapFrom((src, dest) => src.Images
                               .OrderBy(i => i.SortOrder)
                               .Select((i, idx) => new ProductImageDto { Id = i.Id, Url = i.Url, IsMain = idx == 0 })
                               .ToList()));

            // CreateDto → Entity
            CreateMap<CreateProductDto, Product>();   // images are uploaded through the images endpoints

            // UpdateDto → Entity  (null values are ignored — only provided fields are updated)
            CreateMap<UpdateProductDto, Product>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

