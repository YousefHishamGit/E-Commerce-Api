using Application.DTOs.Products;

namespace Application.Contracts.Services
{
    public interface IProductImageService
    {
        Task<List<ProductImageDto>> AddImagesAsync(int productId, IReadOnlyList<ImageUploadDto> files, CancellationToken ct = default);
        Task<List<ProductImageDto>> DeleteImageAsync(int productId, int imageId, CancellationToken ct = default);
    }
}
