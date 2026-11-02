using AutoMapper;
using Application.DTOs.Orders;
using Application.DTOs.Payments;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            // OrderItem → OrderItemDto
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName,
                           opt  => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductPhoto,
                           opt  => opt.MapFrom(src => src.Product != null ? src.Product.Photo : null))
                .ForMember(dest => dest.SubTotal,
                           opt  => opt.MapFrom(src => src.SubTotal));

            // Order → OrderDto
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Status,
                           opt  => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CustomerName,
                           opt  => opt.MapFrom(src => src.Customer != null
                               ? $"{src.Customer.FirstName} {src.Customer.LastName}"
                               : string.Empty))
                .ForMember(dest => dest.CouponCode,
                           opt  => opt.MapFrom(src => src.Coupon != null ? src.Coupon.Code : null))
                .ForMember(dest => dest.Items,
                           opt  => opt.MapFrom(src => src.OrderItems))
                .ForMember(dest => dest.Payments,
                           opt  => opt.MapFrom(src => src.Payments));
        }
    }
}

