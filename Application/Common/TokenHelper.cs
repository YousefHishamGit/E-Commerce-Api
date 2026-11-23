using System.Security.Cryptography;
using System.Text;

namespace Application.Common
{
    public static class TokenHelper
    {
        /// <summary>Cryptographically random, URL-safe token (256 bits).</summary>
        public static string Generate()
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        /// <summary>SHA-256 hex hash. Tokens are high-entropy, so a fast hash is appropriate.</summary>
        public static string Hash(string token)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
