using Application.Contracts.Services;
using Application.DTOs.Common;
using Application.DTOs.Coupons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace E_Commerce.Controllers
{
    public class CouponsController : BaseApiController
    {
        private readonly ICouponService _couponService;

        public CouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }

       
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ApiResponseDto<CouponValidationResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponseDto<CouponValidationResultDto>>> ValidateCoupon(
            [FromBody] ApplyCouponDto dto,
            CancellationToken cancellationToken = default)
        {
            var result = await _couponService.ValidateAndCalculateDiscountAsync(dto, cancellationToken);
            return Ok(ApiResponseDto<CouponValidationResultDto>.Ok(result, result.Message));
        }

        
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<CouponDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<CouponDto>>>> GetAllCoupons(
            CancellationToken cancellationToken = default)
        {
            var coupons = await _couponService.GetAllCouponsAsync(cancellationToken);
            return Ok(ApiResponseDto<IEnumerable<CouponDto>>.Ok(coupons));
        }

       
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<CouponDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<CouponDto>>> GetCouponById(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var coupon = await _couponService.GetCouponByIdAsync(id, cancellationToken);
            return Ok(ApiResponseDto<CouponDto>.Ok(coupon));
        }

       
        [Authorize(Roles = "Admin")]
        [HttpGet("code/{code}")]
        [ProducesResponseType(typeof(ApiResponseDto<CouponDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<CouponDto>>> GetCouponByCode(
            [FromRoute] string code,
            CancellationToken cancellationToken = default)
        {
            var coupon = await _couponService.GetCouponByCodeAsync(code, cancellationToken);
            return Ok(ApiResponseDto<CouponDto>.Ok(coupon));
        }

      
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<CouponDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponseDto<CouponDto>>> CreateCoupon(
            [FromBody] CreateCouponDto dto,
            CancellationToken cancellationToken = default)
        {
            var created = await _couponService.CreateCouponAsync(dto, cancellationToken);
            return CreatedAtAction(
                nameof(GetCouponById),
                new { id = created.Id },
                ApiResponseDto<CouponDto>.Created(created, "Coupon created successfully."));
        }

   
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<CouponDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<CouponDto>>> UpdateCoupon(
            [FromRoute] int id,
            [FromBody] UpdateCouponDto dto,
            CancellationToken cancellationToken = default)
        {
            var updated = await _couponService.UpdateCouponAsync(id, dto, cancellationToken);
            return Ok(ApiResponseDto<CouponDto>.Ok(updated, "Coupon updated successfully."));
        }

     
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:int}/toggle")]
        [ProducesResponseType(typeof(ApiResponseDto<CouponDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<CouponDto>>> ToggleCouponStatus(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var updated = await _couponService.ToggleCouponStatusAsync(id, cancellationToken);
            return Ok(ApiResponseDto<CouponDto>.Ok(updated, $"Coupon status toggled to {(updated.IsActive ? "Active" : "Inactive")}."));
        }

      
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<string>>> DeleteCoupon(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            await _couponService.DeleteCouponAsync(id, cancellationToken);
            return Ok(ApiResponseDto<string>.Ok($"Coupon with ID {id} has been deleted successfully."));
        }
    }
}

