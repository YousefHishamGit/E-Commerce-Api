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
            builder.Services.AddSwaggerServices();

            builder.Services.AddControllers();

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            // ── Build app ────────────────────────────────────────────────────
            var app = builder.Build();

            // ── Global Exception Handler ──────────────────────────────────────
            app.UseExceptionHandler();

            // ── Auto-migrate & seed on startup ───────────────────────────────
            await app.MigrateDatabaseAsync();

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

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
