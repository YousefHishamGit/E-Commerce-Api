using Application.Contracts.Services;
using Application.Contracts.Services.Common;
using Application.Contracts.Services.Hashing;
using Application.Services;
using E_Commerce.Service;
using Infrastructure.Common;

namespace E_Commerce.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // HttpContextAccessor & CurrentUser
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUser>();

            // JWT Settings & Token Generator
            services.Configure<JwtSettings>(configuration.GetSection("JWT"));
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // Password Hasher
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            // Application Services
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IBasketService, BasketService>();
            services.AddScoped<IWishlistService, WishlistService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IOrderService, OrderService>();

            return services;
        }
    }
}
