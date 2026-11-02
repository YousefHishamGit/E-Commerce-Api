using Application.Contracts.Repository;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class CouponRepository : GenericRepository<Coupon, int>, ICouponRepository
    {
        private readonly AppDbContext _context;

        public CouponRepository(AppDbContext context) : base(context)
        {
            _context= context;

        }
        public async Task<bool> CouponCodeExistsAsync(string code)
        {
            return await _context.Coupons.AnyAsync(c => c.Code == code);
            
        }

        public async Task<Coupon?> GetByCodeAsync(string code)
        {
            return await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code);
        }
    }
}
