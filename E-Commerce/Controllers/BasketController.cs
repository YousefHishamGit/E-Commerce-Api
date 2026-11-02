using Application.Contracts.Services;
using Application.DTOs.Basket;
using Application.DTOs.Common;
using Application.DTOs.Coupons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace E_Commerce.Controllers
{
    [Authorize]
    public class BasketController : BaseApiController
    {
        private readonly IBasketService _basketService;

        public BasketController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        /// <summary>
        /// Get the authenticated user's current shopping basket.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<BasketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<BasketDto>>> GetBasket(CancellationToken cancellationToken)
        {
            var basket = await _basketService.GetBasketAsync(cancellationToken);
            return Ok(ApiResponseDto<BasketDto>.Ok(basket));
        }

        /// <summary>
        /// Add an item to the basket (or increment quantity if it already exists).
        /// </summary>
        [HttpPost("items")]
        [ProducesResponseType(typeof(ApiResponseDto<BasketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<BasketDto>>> AddItem(
            [FromBody] AddBasketItemDto dto,
            CancellationToken cancellationToken)
        {
            var basket = await _basketService.AddItemToBasketAsync(
                dto.ProductId,
                dto.Quantity,
                cancellationToken);

            return Ok(ApiResponseDto<BasketDto>.Ok(basket, "Item added to basket successfully."));
        }

        /// <summary>
        /// Update the quantity of an item in the user's basket. If quantity is 0 or less, the item is removed.
        /// </summary>
        [HttpPut("items/{itemId:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<BasketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<BasketDto>>> UpdateItemQuantity(
            [FromRoute] int itemId,
            [FromBody] UpdateBasketItemQuantityDto dto,
            CancellationToken cancellationToken)
        {
            var basket = await _basketService.UpdateItemQuantityAsync(
                itemId,
                dto.Quantity,
                cancellationToken);

            return Ok(ApiResponseDto<BasketDto>.Ok(basket, "Basket item updated successfully."));
        }

        /// <summary>
        /// Remove a specific item from the basket.
        /// </summary>
        [HttpDelete("items/{itemId:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<BasketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<BasketDto>>> RemoveItem(
            [FromRoute] int itemId,
            CancellationToken cancellationToken)
        {
            var basket = await _basketService.RemoveItemFromBasketAsync(
                itemId,
                cancellationToken);

            return Ok(ApiResponseDto<BasketDto>.Ok(basket, "Item removed from basket successfully."));
        }

        /// <summary>
        /// Apply a discount coupon to the user's basket.
        /// </summary>
        [HttpPost("coupon")]
        [ProducesResponseType(typeof(ApiResponseDto<BasketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<BasketDto>>> ApplyCoupon(
            [FromBody] ApplyCouponToBasketDto dto,
            CancellationToken cancellationToken)
        {
            var basket = await _basketService.ApplyCouponToBasketAsync(dto.Code, cancellationToken);
            return Ok(ApiResponseDto<BasketDto>.Ok(basket, $"Coupon '{dto.Code.ToUpper()}' applied successfully."));
        }

        /// <summary>
        /// Remove the applied discount coupon from the user's basket.
        /// </summary>
        [HttpDelete("coupon")]
        [ProducesResponseType(typeof(ApiResponseDto<BasketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<BasketDto>>> RemoveCoupon(CancellationToken cancellationToken)
        {
            var basket = await _basketService.RemoveCouponFromBasketAsync(cancellationToken);
            return Ok(ApiResponseDto<BasketDto>.Ok(basket, "Coupon removed from basket successfully."));
        }

        /// <summary>
        /// Clear all items and reset the user's basket.
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<string>>> ClearBasket(CancellationToken cancellationToken)
        {
            await _basketService.ClearBasketAsync(cancellationToken);
            return Ok(ApiResponseDto<string>.Ok("Basket cleared successfully."));
        }
    }
}
