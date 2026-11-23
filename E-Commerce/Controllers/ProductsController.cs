using Application.Contracts.Services;
using Application.DTOs.Common;
using Application.DTOs.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;
        private readonly IProductImageService _imageService;

        public ProductsController(IProductService productService, IProductImageService imageService)
        {
            _productService = productService;
            _imageService = imageService;
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<ProductDto>>> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            var updated = await _productService.UpdateProductAsync(id, dto);
            return Ok(ApiResponseDto<ProductDto>.Ok(updated, "Product updated successfully."));
        }

        /// <summary>
        /// Upload one or more images for a product (max 4 per product, JPG/PNG/WEBP, 5 MB each).
        /// The first image (lowest free slot) is the main image.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/images")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(25 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 25 * 1024 * 1024)]
        [ProducesResponseType(typeof(ApiResponseDto<List<ProductImageDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<List<ProductImageDto>>>> UploadImages(
            int id, [FromForm] List<IFormFile> files, CancellationToken ct)
        {
            var uploads = files.Select(f => new ImageUploadDto { Content = f.OpenReadStream(), FileName = f.FileName }).ToList();
            try
            {
                var images = await _imageService.AddImagesAsync(id, uploads, ct);
                return Ok(ApiResponseDto<List<ProductImageDto>>.Ok(images, "Images uploaded successfully."));
            }
            finally
            {
                foreach (var u in uploads) await u.Content.DisposeAsync();
            }
        }

        /// <summary>
        /// Delete one image of a product.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}/images/{imageId:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<List<ProductImageDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<List<ProductImageDto>>>> DeleteImage(
            int id, int imageId, CancellationToken ct)
        {
            var images = await _imageService.DeleteImageAsync(id, imageId, ct);
            return Ok(ApiResponseDto<List<ProductImageDto>>.Ok(images, "Image deleted successfully."));
        }

        /// <summary>
        /// Delete a product by its ID.
        /// </summary>
        [Authorize(Roles = "Admin")]
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

