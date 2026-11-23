using Application.Contracts.Services.Hashing;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Extensions
{
    public static class AdminSeedExtensions
    {
        /// <summary>
        /// Creates the first Admin account from configuration (Admin:Email / Admin:Password),
        /// only when no Admin exists yet. Credentials must come from user-secrets or
        /// environment variables (Admin__Email, Admin__Password) - never from committed files.
        /// </summary>
        public static async Task SeedAdminAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
            var config = app.Configuration;

            var email = config["Admin:Email"]?.Trim();
            var password = config["Admin:Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogInformation("Admin seeding skipped (Admin:Email / Admin:Password not configured).");
                return;
            }

            if (password.Length < 8)
            {
                logger.LogWarning("Admin seeding skipped: Admin:Password must be at least 8 characters.");
                return;
            }

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            try
            {
                if (await db.Users.AnyAsync(u => u.Role == UserRole.Admin))
                    return;

                if (await db.Users.AnyAsync(u => u.Email == email))
                {
                    logger.LogWarning("Admin seeding skipped: a user with email {Email} already exists.", email);
                    return;
                }

                db.Users.Add(new User
                {
                    FirstName = config["Admin:FirstName"] ?? "Admin",
                    LastName = config["Admin:LastName"] ?? "User",
                    Phone = config["Admin:Phone"] ?? "01000000000",
                    Gender = Gender.Male,
                    Email = email,
                    HashPassword = hasher.Hash(password),
                    Role = UserRole.Admin,
                    EmailConfirmed = true
                });
                await db.SaveChangesAsync();

                logger.LogInformation("Admin account {Email} created.", email);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Admin seeding failed: {Message}", ex.Message);
            }
        }
    }
}
