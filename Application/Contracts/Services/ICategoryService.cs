using Application.DTOs.Categories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> GetCategoryByIdAsync(int categoryId);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
        Task<CategoryDto> UpdateCategoryAsync(int categoryId, UpdateCategoryDto updateCategoryDto);
        Task DeleteCategoryAsync(int categoryId);
    }
}
