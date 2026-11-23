using Application.Contracts.Services;
using Application.DTOs.Categories;
using Application.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E_Commerce.Controllers
{
    public class CategoriesController : BaseApiController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Get all categories with product count.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<CategoryDto>>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(ApiResponseDto<IEnumerable<CategoryDto>>.Ok(categories));
        }

        /// <summary>
        /// Get a category by its ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<CategoryDto>>> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(ApiResponseDto<CategoryDto>.Ok(category));
        }

        /// <summary>
        /// Create a new category.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<CategoryDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponseDto<CategoryDto>>> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var created = await _categoryService.CreateCategoryAsync(dto);
            return CreatedAtAction(
                nameof(GetCategoryById),
                new { id = created.Id },
                ApiResponseDto<CategoryDto>.Created(created, "Category created successfully."));
        }

        /// <summary>
        /// Update an existing category.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<CategoryDto>>> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            var updated = await _categoryService.UpdateCategoryAsync(id, dto);
            return Ok(ApiResponseDto<CategoryDto>.Ok(updated, "Category updated successfully."));
        }

        /// <summary>
        /// Delete a category by its ID.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<string>>> DeleteCategory(int id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return Ok(ApiResponseDto<string>.Ok($"Category with ID {id} has been deleted successfully."));
        }
    }
}

