using Application.DTOs.Addresses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDto>> GetUserAddressesAsync();
        Task<AddressDto> GetAddressByIdAsync(int addressId);
        Task<AddressDto> CreateAddressAsync(CreateAddressDto createAddressDto);
        Task<AddressDto> UpdateAddressAsync(int addressId, UpdateAddressDto updateAddressDto);
        Task DeleteAddressAsync(int addressId);
    }
}

