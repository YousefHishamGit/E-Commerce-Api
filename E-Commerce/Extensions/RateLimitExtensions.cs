using System.Threading.RateLimiting;
using Application.DTOs.Common;
using Microsoft.AspNetCore.RateLimiting;

namespace E_Commerce.Extensions
{
    public static class RateLimitExtensions
    {
        /// <summary>
        /// Per-IP limits for abuse-prone endpoints.
        ///   "auth"  : login / register / refresh / reset ...  (default 10 per minute)
        ///   "email" : forgot-password / resend-confirmation   (default 3 per 10 minutes)
        /// Override with RateLimiting:AuthPerMinute and RateLimiting:EmailPer10Minutes.
        /// Behind a reverse proxy, enable forwarded headers so the real client IP is used.
        /// </summary>
        public static IServiceCollection AddAppRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            var authLimit = configuration.GetValue("RateLimiting:AuthPerMinute", 10);
            var emailLimit = configuration.GetValue("RateLimiting:EmailPer10Minutes", 3);

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy("auth", ctx => RateLimitPartition.GetFixedWindowLimiter(
                    "auth:" + Client(ctx),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = authLimit,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

                options.AddPolicy("email", ctx => RateLimitPartition.GetFixedWindowLimiter(
                    "email:" + Client(ctx),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = emailLimit,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0
                    }));

                options.OnRejected = async (context, ct) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                        context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsJsonAsync(
                        ApiResponseDto<object?>.Fail("Too many requests. Please slow down and try again later.", 429),
                        new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase },
                        ct);
                };
            });

            return services;
        }

        private static string Client(HttpContext ctx) => ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
