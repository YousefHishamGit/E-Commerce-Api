using Domain.Entities;
using Domain.Enums;

namespace Application.Contracts.Repository
{
    public interface IUserTokenRepository
    {
        /// <summary>Stages the token; persisted by IUnitOfWork.SaveChangesAsync.</summary>
        Task AddAsync(UserToken token);

        /// <summary>Returns the (tracked) token matching the hash, or null.</summary>
        Task<UserToken?> GetByHashAsync(UserTokenType type, string tokenHash);

        /// <summary>Immediately revokes all active tokens of this type for the user.</summary>
        Task RevokeAllAsync(Guid userId, UserTokenType type);

        /// <summary>Immediately deletes the user's EXPIRED tokens (revoked ones are kept until expiry for reuse detection).</summary>
        Task DeleteExpiredAsync(Guid userId);
    }
}
