using StackExchange.Redis;

namespace E_Commerce.Extensions
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddRedisServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration["Redis:ConnectionString"]
                ?? throw new InvalidOperationException("Redis:ConnectionString is not configured.");

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configOptions = ConfigurationOptions.Parse(connectionString);
                configOptions.AbortOnConnectFail = false; // Don't crash if Redis is down on startup
                return ConnectionMultiplexer.Connect(configOptions);
            });

            return services;
        }
    }
}

