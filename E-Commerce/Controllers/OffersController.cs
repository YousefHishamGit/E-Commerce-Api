using Application.Contracts.Services;
using Application.DTOs.Common;
using Application.DTOs.Offers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace E_Commerce.Controllers
{
    public class OffersController : BaseApiController
    {
        private readonly IOfferService _offerService;

        public OffersController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        /// <summary>
        /// Get all currently active promotional offers (Public).
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<OfferDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<OfferDto>>>> GetActiveOffers(
            CancellationToken cancellationToken = default)
        {
            var offers = await _offerService.GetActiveOffersAsync(cancellationToken);
            return Ok(ApiResponseDto<IEnumerable<OfferDto>>.Ok(offers));
        }

        /// <summary>
        /// Get active promotional offer for a specific product if available (Public).
        /// </summary>
        [HttpGet("product/{productId:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<OfferDto?>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto<OfferDto?>>> GetActiveOfferForProduct(
            [FromRoute] int productId,
            CancellationToken cancellationToken = default)
        {
            var offer = await _offerService.GetActiveOfferForProductAsync(productId, cancellationToken);
            return Ok(ApiResponseDto<OfferDto?>.Ok(offer));
        }

        /// <summary>
        /// Get all offers including inactive and expired ones (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<OfferDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<OfferDto>>>> GetAllOffers(
            CancellationToken cancellationToken = default)
        {
            var offers = await _offerService.GetAllOffersAsync(cancellationToken);
            return Ok(ApiResponseDto<IEnumerable<OfferDto>>.Ok(offers));
        }

        /// <summary>
        /// Get a specific offer by its ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<OfferDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<OfferDto>>> GetOfferById(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var offer = await _offerService.GetOfferByIdAsync(id, cancellationToken);
            return Ok(ApiResponseDto<OfferDto>.Ok(offer));
        }

        /// <summary>
        /// Create a new promotional offer (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<OfferDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<OfferDto>>> CreateOffer(
            [FromBody] CreateOfferDto dto,
            CancellationToken cancellationToken = default)
        {
            var created = await _offerService.CreateOfferAsync(dto, cancellationToken);
            return CreatedAtAction(
                nameof(GetOfferById),
                new { id = created.Id },
                ApiResponseDto<OfferDto>.Created(created, "Offer created successfully."));
        }

        /// <summary>
        /// Update an existing promotional offer (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<OfferDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<OfferDto>>> UpdateOffer(
            [FromRoute] int id,
            [FromBody] UpdateOfferDto dto,
            CancellationToken cancellationToken = default)
        {
            var updated = await _offerService.UpdateOfferAsync(id, dto, cancellationToken);
            return Ok(ApiResponseDto<OfferDto>.Ok(updated, "Offer updated successfully."));
        }

        /// <summary>
        /// Delete an offer (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<string>>> DeleteOffer(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            await _offerService.DeleteOfferAsync(id, cancellationToken);
            return Ok(ApiResponseDto<string>.Ok($"Offer with ID {id} has been deleted successfully."));
        }

        /// <summary>
        /// Toggle the active status of an offer (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:int}/toggle")]
        [ProducesResponseType(typeof(ApiResponseDto<OfferDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<OfferDto>>> ToggleOfferStatus(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var updated = await _offerService.ToggleOfferStatusAsync(id, cancellationToken);
            return Ok(ApiResponseDto<OfferDto>.Ok(updated, $"Offer status toggled to {(updated.IsActive ? "Active" : "Inactive")}."));
        }
    }
}

