using Application.Contracts.Repository;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserTokenRepository : IUserTokenRepository
    {
        private readonly AppDbContext _context;

        public UserTokenRepository(AppDbContext context) => _context = context;

        public async Task AddAsync(UserToken token) => await _context.UserTokens.AddAsync(token);

        public Task<UserToken?> GetByHashAsync(UserTokenType type, string tokenHash)
            => _context.UserTokens.FirstOrDefaultAsync(t => t.Type == type && t.TokenHash == tokenHash);

        public async Task RevokeAllAsync(Guid userId, UserTokenType type)
        {
            var now = DateTime.UtcNow;
            await _context.UserTokens
                .Where(t => t.UserId == userId && t.Type == type && t.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now));
        }

        public async Task DeleteExpiredAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            await _context.UserTokens
                .Where(t => t.UserId == userId && t.ExpiresAt < now)
                .ExecuteDeleteAsync();
        }
    }
}
