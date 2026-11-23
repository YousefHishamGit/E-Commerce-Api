using Microsoft.AspNetCore.Mvc;
using E_Commerce.Extensions;
using E_Commerce.Middleware;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Register all services via extension methods ──────────────────
            builder.Services.AddDatabaseServices(builder.Configuration);
            builder.Services.AddRedisServices(builder.Configuration);
            builder.Services.AddRepositoryServices();
            builder.Services.AddApplicationServices(builder.Configuration);
            builder.Services.AddMappingServices();
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddSwaggerServices(builder.Configuration);
            builder.Services.AddAppRateLimiting(builder.Configuration);

            builder.Services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    // Return validation failures in the same envelope as every other error
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errors = context.ModelState
                            .Where(e => e.Value?.Errors.Count > 0)
                            .SelectMany(e => e.Value!.Errors.Select(x =>
                                string.IsNullOrWhiteSpace(x.ErrorMessage) ? $"{e.Key} is invalid." : x.ErrorMessage))
                            .ToList();

                        return new BadRequestObjectResult(
                            Application.DTOs.Common.ApiResponseDto<object?>.Fail("Validation failed.", 400, errors));
                    };
                });

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            // ── Build app ────────────────────────────────────────────────────
            var app = builder.Build();

            // ── Global Exception Handler ──────────────────────────────────────
            app.UseExceptionHandler();

            // ── Auto-migrate & seed on startup ───────────────────────────────
            if (app.Environment.IsDevelopment())
                await app.MigrateDatabaseAsync();

            await app.SeedAdminAsync();

            // ── Middleware pipeline ──────────────────────────────────────────
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API v1");
                    c.RoutePrefix = "swagger";
                });

                
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();   // serves /uploads/*

            app.UseCors("AllowFrontend");

            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
