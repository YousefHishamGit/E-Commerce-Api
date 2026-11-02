using Application.Contracts.Services.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int workFactor = 12;
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
        }

        public bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
