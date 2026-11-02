using Application.Contracts.Repository;
using Infrastructure.Repositories;

namespace E_Commerce.Extensions
{
    public static class RepositoryExtensions
    {
        public static IServiceCollection AddRepositoryServices(
            this IServiceCollection services)
        {
            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Specialized Repositories
            services.AddScoped<IBasketRepository, BasketRepository>();
            services.AddScoped<ICacheRepository, CacheRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICouponRepository, CouponRepository>();

            return services;
        }
    }
}

