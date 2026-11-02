using Application.Contracts.Services;
using Application.DTOs.Common;
using Application.DTOs.Products;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Get all products with search, filtering by category/price, sorting, and pagination.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<PaginatedResponseDto<ProductDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto<PaginatedResponseDto<ProductDto>>>> GetProducts(
            [FromQuery] ProductQueryParams queryParams)
        {
            var result = await _productService.GetAllProductsAsync(queryParams);
            return Ok(ApiResponseDto<PaginatedResponseDto<ProductDto>>.Ok(result));
        }

        /// <summary>
        /// Get a product by its ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<ProductDto>>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return Ok(ApiResponseDto<ProductDto>.Ok(product));
        }

        /// <summary>
        /// Create a new product.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<ProductDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponseDto<ProductDto>>> CreateProduct([FromBody] CreateProductDto dto)
        {
            var created = await _productService.CreateProductAsync(dto);
            return CreatedAtAction(
                nameof(GetProduct),
                new { id = created.Id },
                ApiResponseDto<ProductDto>.Created(created, "Product created successfully."));
        }

        /// <summary>
        /// Update an existing product.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<ProductDto>>> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            var updated = await _productService.UpdateProductAsync(id, dto);
            return Ok(ApiResponseDto<ProductDto>.Ok(updated, "Product updated successfully."));
        }

        /// <summary>
        /// Delete a product by its ID.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<string>>> DeleteProduct(int id)
        {
            await _productService.DeleteProductAsync(id);
            return Ok(ApiResponseDto<string>.Ok($"Product with ID {id} has been deleted successfully."));
        }
    }
}

