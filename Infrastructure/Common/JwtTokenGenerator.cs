using Application.Contracts.Services.Hashing;
using Domain.Entities;
using Microsoft.Extensions.Options;
//using Microsoft.IdentityModel.JsonWebTokens;

using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenGenerator(IOptions<JwtSettings> jwtOpts) { 
            _jwtSettings = jwtOpts.Value;
        }
        public (string Token, DateTime ExpiresAt) GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

            var token = new JwtSecurityToken(
              issuer: _jwtSettings.Issuer,
              audience: _jwtSettings.Audience,
              claims: claims,
              expires: expiresAt,
              signingCredentials: credentials
            );
            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
