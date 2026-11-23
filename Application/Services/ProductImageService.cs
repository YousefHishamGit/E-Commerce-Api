using Application.Contracts.Repository;
using Application.Contracts.Services;
using Application.Contracts.Services.Common;
using Application.DTOs.Products;
using Application.Exceptions;
using Application.Specifications;
using Domain.Entities;

namespace Application.Services
{
    public class ProductImageService : IProductImageService
    {
        public const long MaxFileSizeBytes = 5 * 1024 * 1024;   // 5 MB per image
        private const string Folder = "products";

        private readonly IUnitOfWork _uow;
        private readonly IFileStorage _storage;

        public ProductImageService(IUnitOfWork uow, IFileStorage storage)
        {
            _uow = uow;
            _storage = storage;
        }

        public async Task<List<ProductImageDto>> AddImagesAsync(
            int productId, IReadOnlyList<ImageUploadDto> files, CancellationToken ct = default)
        {
            if (files.Count == 0)
                throw new BadRequestException("At least one image file is required.");

            var product = await LoadProductAsync(productId);

            if (product.Images.Count + files.Count > ProductImage.MaxPerProduct)
                throw new BadRequestException(
                    $"A product can have at most {ProductImage.MaxPerProduct} images " +
                    $"(it has {product.Images.Count}, you tried to add {files.Count}).");

            // Validate everything before touching the disk
            var validated = new List<(Stream Content, string Extension)>();
            foreach (var file in files)
                validated.Add(await ValidateAsync(file, ct));

            var usedSlots = product.Images.Select(i => i.SortOrder).ToHashSet();
            var savedUrls = new List<string>();

            try
            {
                foreach (var (content, extension) in validated)
                {
                    var slot = Enumerable.Range(0, ProductImage.MaxPerProduct).First(s => !usedSlots.Contains(s));
                    usedSlots.Add(slot);

                    var url = await _storage.SaveAsync(content, extension, Folder, ct);
                    savedUrls.Add(url);
                    product.Images.Add(new ProductImage { Url = url, SortOrder = slot });
                }

                product.Photo = MainUrl(product.Images);
                product.UpdatedAt = DateTime.UtcNow;
                await _uow.SaveChangesAsync();
            }
            catch
            {
                foreach (var url in savedUrls)
                    await _storage.DeleteAsync(url);   // don't leave orphan files
                throw;
            }

            return ToDtos(product.Images);
        }

        public async Task<List<ProductImageDto>> DeleteImageAsync(int productId, int imageId, CancellationToken ct = default)
        {
            var product = await LoadProductAsync(productId);

            var image = product.Images.FirstOrDefault(i => i.Id == imageId);
            if (image is null)
                throw new NotFoundException($"Image {imageId} was not found on product {productId}.");

            _uow.GetRepository<ProductImage, int>().Delete(image);
            var remaining = product.Images.Where(i => i.Id != imageId).ToList();

            product.Photo = MainUrl(remaining);
            product.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync();

            await _storage.DeleteAsync(image.Url);

            return ToDtos(remaining);
        }

        // ── helpers ───────────────────────────────────────────────────────────

        private async Task<Product> LoadProductAsync(int productId)
        {
            var product = await _uow.GetRepository<Product, int>()
                .GetByIdAsync(productId, new ProductsWithCategorySpec(productId));
            return product ?? throw new NotFoundException($"Product with ID {productId} was not found.");
        }

        private static string? MainUrl(IEnumerable<ProductImage> images)
            => images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url;

        private static List<ProductImageDto> ToDtos(IEnumerable<ProductImage> images)
            => images.OrderBy(i => i.SortOrder)
                     .Select((i, idx) => new ProductImageDto { Id = i.Id, Url = i.Url, IsMain = idx == 0 })
                     .ToList();

        /// <summary>Checks size and real file type (magic bytes), ignoring the client-provided name/extension.</summary>
        private static async Task<(Stream Content, string Extension)> ValidateAsync(ImageUploadDto file, CancellationToken ct)
        {
            var content = file.Content;
            if (!content.CanSeek)
            {
                var buffer = new MemoryStream();
                await content.CopyToAsync(buffer, ct);
                content = buffer;
            }
            content.Position = 0;

            if (content.Length == 0)
                throw new BadRequestException($"File '{file.FileName}' is empty.");
            if (content.Length > MaxFileSizeBytes)
                throw new BadRequestException($"File '{file.FileName}' is larger than {MaxFileSizeBytes / (1024 * 1024)} MB.");

            var header = new byte[12];
            var read = await content.ReadAsync(header.AsMemory(0, header.Length), ct);
            content.Position = 0;

            string? extension = null;
            if (read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                extension = ".jpg";
            else if (read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                     && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                extension = ".png";
            else if (read >= 12 && header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F'
                     && header[8] == 'W' && header[9] == 'E' && header[10] == 'B' && header[11] == 'P')
                extension = ".webp";

            if (extension is null)
                throw new BadRequestException($"File '{file.FileName}' is not a valid image. Allowed types: JPG, PNG, WEBP.");

            return (content, extension);
        }
    }
}
