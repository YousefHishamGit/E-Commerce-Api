using Application.DTOs.Coupons;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
    public interface ICouponService
    {
        
        Task<CouponValidationResultDto> ValidateAndCalculateDiscountAsync(ApplyCouponDto dto, CancellationToken cancellationToken = default);
        Task<IEnumerable<CouponDto>> GetAllCouponsAsync(CancellationToken cancellationToken = default);
        Task<CouponDto> GetCouponByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CouponDto> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<CouponDto> CreateCouponAsync(CreateCouponDto dto, CancellationToken cancellationToken = default);
        Task<CouponDto> UpdateCouponAsync(int id, UpdateCouponDto dto, CancellationToken cancellationToken = default);
        Task DeleteCouponAsync(int id, CancellationToken cancellationToken = default);
        Task<CouponDto> ToggleCouponStatusAsync(int id, CancellationToken cancellationToken = default);
    }
}

