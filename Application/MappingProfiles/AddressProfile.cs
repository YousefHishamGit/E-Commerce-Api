using AutoMapper;
using Application.DTOs.Addresses;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class AddressProfile : Profile
    {
        public AddressProfile()
        {
            // Entity → DTO
            CreateMap<Address, AddressDto>();

            // CreateDto → Entity
            CreateMap<CreateAddressDto, Address>();

            // UpdateDto → Entity
            CreateMap<UpdateAddressDto, Address>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

