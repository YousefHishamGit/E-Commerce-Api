using Application.DTOs.Wishlist;

namespace Application.Contracts.Services
{
    public interface IWishlistService
    {
      
        Task<WishlistDto> GetUserWishlistAsync(CancellationToken cancellationToken = default);


        Task<WishlistDto> AddProductToWishlistAsync(int productId, CancellationToken cancellationToken = default);

        Task<WishlistDto> RemoveProductFromWishlistAsync(int productId, CancellationToken cancellationToken = default);

        Task<bool> IsProductInWishlistAsync(int productId, CancellationToken cancellationToken = default);

        Task<bool> ClearWishlistAsync(CancellationToken cancellationToken = default);
    }
}

