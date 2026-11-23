using Application.Contracts.Repository;
using Application.Contracts.Services;
using Application.DTOs.Coupons;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CouponService : ICouponService
    {
        private readonly ICouponRepository _couponRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CouponService(
            ICouponRepository couponRepo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _couponRepo = couponRepo;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<CouponValidationResultDto> ValidateAndCalculateDiscountAsync(ApplyCouponDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
            {
                return new CouponValidationResultDto
                {
                    IsValid = false,
                    Message = "Coupon code is required.",
                    DiscountAmount = 0,
                    FinalAmount = dto.OrderAmount
                };
            }

            var coupon = await _couponRepo.GetByCodeAsync(dto.Code.Trim().ToUpperInvariant());
            if (coupon is null)
            {
                return new CouponValidationResultDto
                {
                    IsValid = false,
                    Message = $"Coupon with code '{dto.Code}' was not found.",
                    DiscountAmount = 0,
                    FinalAmount = dto.OrderAmount
                };
            }

            if (!coupon.IsActive)
            {
                return new CouponValidationResultDto
                {
                    IsValid = false,
                    Message = "This coupon is inactive.",
                    DiscountAmount = 0,
                    FinalAmount = dto.OrderAmount
                };
            }

            var now = DateTime.UtcNow;
            if (coupon.StartDate > now)
            {
                return new CouponValidationResultDto
                {
                    IsValid = false,
                    Message = "This coupon is not active yet.",
                    DiscountAmount = 0,
                    FinalAmount = dto.OrderAmount
                };
            }

            if (coupon.EndDate < now)
            {
                return new CouponValidationResultDto
                {
                    IsValid = false,
                    Message = "This coupon has expired.",
                    DiscountAmount = 0,
                    FinalAmount = dto.OrderAmount
                };
            }

            if (coupon.MaxUsage.HasValue && coupon.UsedCount >= coupon.MaxUsage.Value)
            {
                return new CouponValidationResultDto
                {
                    IsValid = false,
                    Message = "This coupon has reached its maximum usage limit.",
                    DiscountAmount = 0,
                    FinalAmount = dto.OrderAmount
                };
            }

            if (coupon.MinOrderAmount.HasValue && dto.OrderAmount < coupon.MinOrderAmount.Value)
            {
                return new CouponValidationResultDto
                {
                    IsValid = false,
                    Message = $"Minimum order amount of {coupon.MinOrderAmount.Value:C} is required to apply this coupon.",
                    DiscountAmount = 0,
                    FinalAmount = dto.OrderAmount
                };
            }

            decimal discountAmount = coupon.Type == CouponType.Percentage
                ? Math.Round((dto.OrderAmount * coupon.Value) / 100m, 2)
                : Math.Min(coupon.Value, dto.OrderAmount);

            decimal finalAmount = dto.OrderAmount - discountAmount;
            if (finalAmount < 0) finalAmount = 0;

            return new CouponValidationResultDto
            {
                IsValid = true,
                Message = "Coupon is valid.",
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount
            };
        }

        public async Task<IEnumerable<CouponDto>> GetAllCouponsAsync(CancellationToken cancellationToken = default)
        {
            var coupons = await _couponRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<CouponDto>>(coupons);
        }

        public async Task<CouponDto> GetCouponByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var coupon = await _couponRepo.GetByIdAsync(id);
            if (coupon is null)
                throw new NotFoundException($"Coupon with ID {id} was not found.");

            return _mapper.Map<CouponDto>(coupon);
        }

        public async Task<CouponDto> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            var coupon = await _couponRepo.GetByCodeAsync(code.Trim().ToUpperInvariant());
            if (coupon is null)
                throw new NotFoundException($"Coupon with code '{code}' was not found.");

            return _mapper.Map<CouponDto>(coupon);
        }

        public async Task<CouponDto> CreateCouponAsync(CreateCouponDto dto, CancellationToken cancellationToken = default)
        {
            var normalizedCode = dto.Code.Trim().ToUpper();

            if (await _couponRepo.CouponCodeExistsAsync(normalizedCode))
                throw new ConflictException($"Coupon with code '{normalizedCode}' already exists.");

            if (dto.EndDate <= dto.StartDate)
                throw new BadRequestException("EndDate must be after StartDate.");

            if (!Enum.TryParse<CouponType>(dto.Type, true, out var couponType))
                throw new BadRequestException("Invalid coupon type. Must be 'Percentage' or 'FixedAmount'.");

            if (couponType == CouponType.Percentage && (dto.Value <= 0 || dto.Value > 100))
                throw new BadRequestException("Percentage discount must be between 1 and 100.");

            var coupon = _mapper.Map<Coupon>(dto);
            coupon.Code = normalizedCode;
            coupon.Type = couponType;
            coupon.IsActive = true;
            coupon.UsedCount = 0;

            await _couponRepo.AddAsync(coupon);
            await _uow.SaveChangesAsync();

            return _mapper.Map<CouponDto>(coupon);
        }

        public async Task<CouponDto> UpdateCouponAsync(int id, UpdateCouponDto dto, CancellationToken cancellationToken = default)
        {
            var coupon = await _couponRepo.GetByIdAsync(id);
            if (coupon is null)
                throw new NotFoundException($"Coupon with ID {id} was not found.");

            if (dto.EndDate.HasValue && dto.EndDate.Value <= coupon.StartDate)
                throw new BadRequestException("EndDate must be after StartDate.");

            if (dto.Value.HasValue && coupon.Type == CouponType.Percentage && (dto.Value.Value <= 0 || dto.Value.Value > 100))
                throw new BadRequestException("Percentage discount must be between 1 and 100.");

            _mapper.Map(dto, coupon);
            _couponRepo.Update(coupon);
            await _uow.SaveChangesAsync();

            return _mapper.Map<CouponDto>(coupon);
        }

        public async Task DeleteCouponAsync(int id, CancellationToken cancellationToken = default)
        {
            var coupon = await _couponRepo.GetByIdAsync(id);
            if (coupon is null)
                throw new NotFoundException($"Coupon with ID {id} was not found.");

            _couponRepo.Delete(coupon);
            await _uow.SaveChangesAsync();
        }

        public async Task<CouponDto> ToggleCouponStatusAsync(int id, CancellationToken cancellationToken = default)
        {
            var coupon = await _couponRepo.GetByIdAsync(id);
            if (coupon is null)
                throw new NotFoundException($"Coupon with ID {id} was not found.");

            coupon.IsActive = !coupon.IsActive;
            _couponRepo.Update(coupon);
            await _uow.SaveChangesAsync();

            return _mapper.Map<CouponDto>(coupon);
        }
    }
}

