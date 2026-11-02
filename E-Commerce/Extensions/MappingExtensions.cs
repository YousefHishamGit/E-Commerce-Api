using Application.MappingProfiles;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Extensions
{
    public static class MappingExtensions
    {
        public static IServiceCollection AddMappingServices(
            this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(ProductProfile).Assembly);
            });

            return services;
        }
    }
}

