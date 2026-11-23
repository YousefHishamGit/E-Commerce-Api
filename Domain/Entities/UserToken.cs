using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Single-use, expiring token (refresh / email confirmation / password reset).
    /// Only the SHA-256 hash is stored, never the raw token.
    /// </summary>
    public class UserToken : BaseEntity<int>
    {
        public UserTokenType Type { get; set; }
        public string TokenHash { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }

        /// <summary>Set when the token is used or revoked. A non-null value means it can no longer be used.</summary>
        public DateTime? RevokedAt { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;
    }
}
