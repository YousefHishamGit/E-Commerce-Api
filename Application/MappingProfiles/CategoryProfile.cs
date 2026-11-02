using AutoMapper;
using Application.DTOs.Categories;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            // Entity → DTO
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ProductCount,
                           opt  => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0));

            // CreateDto → Entity
            CreateMap<CreateCategoryDto, Category>();

            // UpdateDto → Entity
            CreateMap<UpdateCategoryDto, Category>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

