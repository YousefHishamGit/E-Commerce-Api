using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace E_Commerce.Extensions
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtKey = configuration["JWT:Key"]
                ?? throw new InvalidOperationException("JWT:Key is not configured.");

            var issuer   = configuration["JWT:Issuer"];
            var audience = configuration["JWT:Audience"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = issuer,
                    ValidAudience            = audience,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew                = TimeSpan.FromMinutes(5) 
                };

                // Return 401 JSON instead of redirect
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode  = 401;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(
                            """{"success":false,"message":"Unauthorized. Please login first.","statusCode":401}""");
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode  = 403;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(
                            """{"success":false,"message":"Forbidden. You do not have permission.","statusCode":403}""");
                    }
                };
            });

            services.AddAuthorization();

            return services;
        }
    }
}

