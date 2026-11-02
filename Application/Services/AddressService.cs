using Application.Contracts.Repository;
using Application.Contracts.Services;
using Application.Contracts.Services.Common;
using Application.DTOs.Addresses;
using Application.Exceptions;
using Application.Specifications;
using AutoMapper;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AddressService : IAddressService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;

        public AddressService(IUnitOfWork uow, IMapper mapper, ICurrentUser currentUser)
        {
            _uow = uow;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<IEnumerable<AddressDto>> GetUserAddressesAsync()
        {
            var spec = new AddressesByUserSpec(_currentUser.UserId);
            var addresses = await _uow.GetRepository<Address, int>().GetAllAsync(spec);
            return _mapper.Map<IEnumerable<AddressDto>>(addresses);
        }

        public async Task<AddressDto> GetAddressByIdAsync(int addressId)
        {
            var spec = new AddressesByUserSpec(addressId, _currentUser.UserId);
            var address = await _uow.GetRepository<Address, int>().GetByIdAsync(addressId, spec);
            if (address is null)
                throw new NotFoundException($"Address with ID {addressId} was not found.");

            return _mapper.Map<AddressDto>(address);
        }

        public async Task<AddressDto> CreateAddressAsync(CreateAddressDto createAddressDto)
        {
            var address = _mapper.Map<Address>(createAddressDto);
            address.UserId = _currentUser.UserId;

            await _uow.GetRepository<Address, int>().AddAsync(address);
            await _uow.SaveChangesAsync();

            return _mapper.Map<AddressDto>(address);
        }

        public async Task<AddressDto> UpdateAddressAsync(int addressId, UpdateAddressDto updateAddressDto)
        {
            var repo = _uow.GetRepository<Address, int>();
            var spec = new AddressesByUserSpec(addressId, _currentUser.UserId);

            var address = await repo.GetByIdAsync(addressId, spec);
            if (address is null)
                throw new NotFoundException($"Address with ID {addressId} was not found.");

            _mapper.Map(updateAddressDto, address);
            repo.Update(address);
            await _uow.SaveChangesAsync();

            return _mapper.Map<AddressDto>(address);
        }

        public async Task DeleteAddressAsync(int addressId)
        {
            var repo = _uow.GetRepository<Address, int>();
            var spec = new AddressesByUserSpec(addressId, _currentUser.UserId);

            var address = await repo.GetByIdAsync(addressId, spec);
            if (address is null)
                throw new NotFoundException($"Address with ID {addressId} was not found.");

            repo.Delete(address);
            await _uow.SaveChangesAsync();
        }
    }
}

