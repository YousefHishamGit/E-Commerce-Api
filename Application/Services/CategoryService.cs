using Application.Contracts.Repository;
using Application.Contracts.Services;
using Application.DTOs.Categories;
using Application.Exceptions;
using Application.Specifications;
using AutoMapper;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var spec = new CategoriesWithProductsSpec();
            var categories = await _uow.GetRepository<Category, int>().GetAllAsync(spec);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int categoryId)
        {
            var spec = new CategoriesWithProductsSpec(categoryId);
            var category = await _uow.GetRepository<Category, int>().GetByIdAsync(categoryId, spec);
            if (category is null)
                throw new NotFoundException($"Category with ID {categoryId} was not found.");

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            var category = _mapper.Map<Category>(createCategoryDto);
            await _uow.GetRepository<Category, int>().AddAsync(category);
            await _uow.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int categoryId, UpdateCategoryDto updateCategoryDto)
        {
            var repository = _uow.GetRepository<Category, int>();
            var category = await repository.GetByIdAsync(categoryId);
            if (category is null)
                throw new NotFoundException($"Category with ID {categoryId} was not found.");

            _mapper.Map(updateCategoryDto, category);
            repository.Update(category);
            await _uow.SaveChangesAsync();

            var spec = new CategoriesWithProductsSpec(category.Id);
            var updated = await repository.GetByIdAsync(category.Id, spec);
            return _mapper.Map<CategoryDto>(updated ?? category);
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            var categoryRepo = _uow.GetRepository<Category, int>();
            var spec = new CategoriesWithProductsSpec(categoryId);
            var category = await categoryRepo.GetByIdAsync(categoryId, spec);
            if (category is null)
                throw new NotFoundException($"Category with ID {categoryId} was not found.");

            if (category.Products != null && category.Products.Any())
                throw new BadRequestException($"Cannot delete category '{category.Name}' because it has {category.Products.Count} associated product(s). Please reassign or delete the products first.");

            categoryRepo.Delete(category);
            await _uow.SaveChangesAsync();
        }
    }
}

