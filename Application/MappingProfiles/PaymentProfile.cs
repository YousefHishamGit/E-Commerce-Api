using AutoMapper;
using Application.DTOs.Payments;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            // Entity → DTO  (enums → strings)
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.Method,
                           opt  => opt.MapFrom(src => src.Method.ToString()))
                .ForMember(dest => dest.Status,
                           opt  => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}

