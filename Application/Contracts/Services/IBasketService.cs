using Application.DTOs.Basket;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
    public interface IBasketService
    {
        Task<BasketDto> GetBasketAsync(CancellationToken cancellationToken = default);
        Task<BasketDto> AddItemToBasketAsync(int productId, int quantity, CancellationToken cancellationToken = default);
        Task<BasketDto> RemoveItemFromBasketAsync(int basketItemId, CancellationToken cancellationToken = default);
        Task<BasketDto> UpdateItemQuantityAsync(int basketItemId, int quantity, CancellationToken cancellationToken = default);
        Task<BasketDto> ApplyCouponToBasketAsync(string couponCode, CancellationToken cancellationToken = default);
        Task<BasketDto> RemoveCouponFromBasketAsync(CancellationToken cancellationToken = default);
        Task<bool> ClearBasketAsync(CancellationToken cancellationToken = default);
    }
}
