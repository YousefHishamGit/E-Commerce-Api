using Application.Contracts.Services;
using Application.DTOs.Addresses;
using Application.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E_Commerce.Controllers
{
    [Authorize]
    public class AddressesController : BaseApiController
    {
        private readonly IAddressService _addressService;

        public AddressesController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<AddressDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<AddressDto>>>> GetMyAddresses()
        {
            var addresses = await _addressService.GetUserAddressesAsync();
            return Ok(ApiResponseDto<IEnumerable<AddressDto>>.Ok(addresses));
        }

      
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<AddressDto>>> GetAddressById(int id)
        {
            var address = await _addressService.GetAddressByIdAsync(id);
            return Ok(ApiResponseDto<AddressDto>.Ok(address));
        }

     
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<AddressDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<AddressDto>>> CreateAddress([FromBody] CreateAddressDto dto)
        {
            var created = await _addressService.CreateAddressAsync(dto);
            return CreatedAtAction(
                nameof(GetAddressById),
                new { id = created.Id },
                ApiResponseDto<AddressDto>.Created(created, "Address added successfully."));
        }

    
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<AddressDto>>> UpdateAddress(int id, [FromBody] UpdateAddressDto dto)
        {
            var updated = await _addressService.UpdateAddressAsync(id, dto);
            return Ok(ApiResponseDto<AddressDto>.Ok(updated, "Address updated successfully."));
        }

        
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<string>>> DeleteAddress(int id)
        {
            await _addressService.DeleteAddressAsync(id);
            return Ok(ApiResponseDto<string>.Ok($"Address with ID {id} has been deleted successfully."));
        }
    }
}

