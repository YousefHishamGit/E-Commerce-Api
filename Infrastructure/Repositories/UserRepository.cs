using Application.Contracts.Repository;
using Application.Specifications;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository :GenericRepository<User,Guid>, IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context):base(context) {
            _context = context;
        }
      
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email); // email is case-sensitive
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        }

    }
}
