using Application.DTOs.Common;
using Application.DTOs.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
    public interface IProductService
    {
        Task<PaginatedResponseDto<ProductDto>> GetAllProductsAsync(ProductQueryParams queryParams);
        Task<ProductDto> GetProductByIdAsync(int productId);
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductDto> UpdateProductAsync(int productId, UpdateProductDto updateProductDto);
        Task DeleteProductAsync(int productId);
    }
}
