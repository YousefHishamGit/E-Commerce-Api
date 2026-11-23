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

            // Store pricing rules (shipping / tax) from the "Shop" section
            services.AddSingleton(configuration.GetSection("Shop").Get<Application.Common.ShopSettings>()
                                  ?? new Application.Common.ShopSettings());

            // Paymob (Egypt) payments: keys come from user-secrets / environment variables
            services.AddSingleton(configuration.GetSection("Paymob").Get<Application.Common.PaymobSettings>()
                                  ?? new Application.Common.PaymobSettings());
            services.AddHttpClient<Application.Contracts.Services.Common.IPaymentGateway, PaymobGateway>();
            services.AddHostedService<E_Commerce.Service.UnpaidOrderCleanupService>();

            // Auth behaviour (lockout, token lifetimes, frontend URL for email links)
            services.AddSingleton(configuration.GetSection("Auth").Get<Application.Common.AuthSettings>()
                                  ?? new Application.Common.AuthSettings());

            // Email: real SMTP when Email:Host is configured, otherwise log-only
            services.Configure<EmailSettings>(configuration.GetSection("Email"));
            if (!string.IsNullOrWhiteSpace(configuration["Email:Host"]))
                services.AddScoped<Application.Contracts.Services.Common.IEmailSender, SmtpEmailSender>();
            else
                services.AddScoped<Application.Contracts.Services.Common.IEmailSender, LoggingEmailSender>();

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
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IProductImageService, ProductImageService>();
            services.AddScoped<IPaymentService, PaymentService>();

            // File storage: local disk under wwwroot/uploads (served as static files at /uploads)
            services.AddSingleton<IFileStorage>(sp =>
            {
                var env = sp.GetRequiredService<IWebHostEnvironment>();
                var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
                return new LocalFileStorage(Path.Combine(webRoot, "uploads"), "/uploads");
            });

            return services;
        }
    }
}
