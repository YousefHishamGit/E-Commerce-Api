using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Repository
{
    public interface ICouponRepository:IGenericRepository<Coupon,int>
    {
        Task<Coupon?> GetByCodeAsync(string code);
        Task<bool> CouponCodeExistsAsync(string code);
    }
}
