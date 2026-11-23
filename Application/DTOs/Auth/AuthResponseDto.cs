namespace Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool EmailConfirmed { get; set; }

        /// <summary>Short-lived JWT access token.</summary>
        public string Token { get; set; } = null!;
        public DateTime TokenExpiry { get; set; }

        /// <summary>Long-lived, single-use token to obtain a new access token via /api/Auth/refresh.</summary>
        public string RefreshToken { get; set; } = null!;
        public DateTime RefreshTokenExpiry { get; set; }
    }
}
