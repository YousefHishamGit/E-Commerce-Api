using Application.Contracts.Services;
using Application.DTOs.Common;
using Application.DTOs.Wishlist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Authorize]
    public class WishlistController : BaseApiController
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

       
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<WishlistDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<WishlistDto>>> GetWishlist(CancellationToken cancellationToken)
        {
            var wishlist = await _wishlistService.GetUserWishlistAsync(cancellationToken);
            return Ok(ApiResponseDto<WishlistDto>.Ok(wishlist));
        }

       
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<WishlistDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponseDto<WishlistDto>>> AddToWishlist(
            [FromBody] AddToWishlistDto dto,
            CancellationToken cancellationToken)
        {
            var wishlist = await _wishlistService.AddProductToWishlistAsync(dto.ProductId, cancellationToken);
            return Ok(ApiResponseDto<WishlistDto>.Ok(wishlist, "Product added to wishlist successfully."));
        }

       
        [HttpDelete("{productId:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<WishlistDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<WishlistDto>>> RemoveFromWishlist(
            [FromRoute] int productId,
            CancellationToken cancellationToken)
        {
            var wishlist = await _wishlistService.RemoveProductFromWishlistAsync(productId, cancellationToken);
            return Ok(ApiResponseDto<WishlistDto>.Ok(wishlist, "Product removed from wishlist successfully."));
        }

      
        [HttpGet("check/{productId:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<bool>>> CheckIsInWishlist(
            [FromRoute] int productId,
            CancellationToken cancellationToken)
        {
            var isInWishlist = await _wishlistService.IsProductInWishlistAsync(productId, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(isInWishlist));
        }

        
        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<string>>> ClearWishlist(CancellationToken cancellationToken)
        {
            await _wishlistService.ClearWishlistAsync(cancellationToken);
            return Ok(ApiResponseDto<string>.Ok("Wishlist cleared successfully."));
        }
    }
}

