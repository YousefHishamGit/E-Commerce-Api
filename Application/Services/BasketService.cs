using Application.Contracts.Repository;
using Application.Contracts.Services;
using Application.Contracts.Services.Common;
using Application.DTOs.Basket;
using Application.Exceptions;
using Application.Specifications;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    public class BasketService : IBasketService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _uow;
        private readonly ICouponRepository _couponRepo;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;

        public BasketService(
            IBasketRepository basketRepo,
            IUnitOfWork uow,
            ICouponRepository couponRepo,
            ICurrentUser currentUser,
            IMapper mapper)
        {
            _basketRepo = basketRepo;
            _uow = uow;
            _couponRepo = couponRepo;
            _currentUser = currentUser;
            _mapper = mapper;
        }

     
        public async Task<BasketDto> GetBasketAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId;
            var key = CacheKeys.Basket(userId);
            var basket = await _basketRepo.GetBasketAsync(key);

            return basket is null
                ? new BasketDto { Id = key }
                : _mapper.Map<BasketDto>(basket);
        }
        public async Task<BasketDto> AddItemToBasketAsync(int productId, int quantity, CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                throw new BadRequestException("Quantity must be greater than zero.");

            var product = await _uow.GetRepository<Product, int>().GetByIdAsync(productId);
            if (product is null)
                throw new NotFoundException($"Product with ID {productId} was not found.");

            if (product.StockQuantity <= 0)
                throw new BadRequestException($"Product '{product.Name}' is out of stock.");

            var userId = _currentUser.UserId;
            var key = CacheKeys.Basket(userId);
            var basket = await _basketRepo.GetBasketAsync(key)
                         ?? new CustomerBasket { Id = key, Items = new List<BasketItem>() };

            // Check if there is an active offer for this product or its category
            var activeOffers = await _uow.GetRepository<Offer, int>().GetAllAsync(new ActiveOffersSpec());
            var applicableOffers = activeOffers.Where(o =>
                (o.ProductId.HasValue && o.ProductId.Value == product.Id) ||
                (o.CategoryId.HasValue && o.CategoryId.Value == product.CategoryId)
            ).ToList();

            var bestOffer = applicableOffers.OrderByDescending(o => o.DiscountPercentage).FirstOrDefault();
            var unitPrice = product.Price;
            if (bestOffer != null)
            {
                var discountAmount = Math.Round((product.Price * bestOffer.DiscountPercentage) / 100m, 2);
                unitPrice = product.Price - discountAmount;
            }

            // If item already exists → increment quantity and refresh price
            var existingItem = basket.Items.FirstOrDefault(i => i.Id == product.Id);
            if (existingItem is not null)
            {
                existingItem.Quantity += quantity;
                existingItem.Price = unitPrice;
            }
            else
            {
                basket.Items.Add(new BasketItem
                {
                    Id = product.Id,
                    ProductName = product.Name,
                    PictureUrl = product.Photo ?? string.Empty,
                    Price = unitPrice,
                    Quantity = quantity
                });
            }

            await ReevaluateCouponDiscountAsync(basket);
            RecalculateTotals(basket);

            var saved = await _basketRepo.CreateOrUpdateBasketAsync(basket, CacheKeys.BasketExpiration);
            if (saved is null)
                throw new BadRequestException("Could not save basket. Please try again.");

            return _mapper.Map<BasketDto>(saved);
        }

     
        public async Task<BasketDto> RemoveItemFromBasketAsync(int basketItemId, CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId;
            var key = CacheKeys.Basket(userId);
            var basket = await _basketRepo.GetBasketAsync(key);

            if (basket is null || !basket.Items.Any())
                throw new NotFoundException("Basket is empty or does not exist.");

            var item = basket.Items.FirstOrDefault(i => i.Id == basketItemId);
            if (item is null)
                throw new NotFoundException($"Item with ID {basketItemId} was not found in the basket.");

            basket.Items.Remove(item);

            await ReevaluateCouponDiscountAsync(basket);
            RecalculateTotals(basket);

            var saved = await _basketRepo.CreateOrUpdateBasketAsync(basket, CacheKeys.BasketExpiration);
            if (saved is null)
                throw new BadRequestException("Could not update basket. Please try again.");

            return _mapper.Map<BasketDto>(saved);
        }

    
        public async Task<BasketDto> UpdateItemQuantityAsync(int basketItemId, int quantity, CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId;
            var key = CacheKeys.Basket(userId);
            var basket = await _basketRepo.GetBasketAsync(key);

            if (basket is null || !basket.Items.Any())
                throw new NotFoundException("Basket is empty or does not exist.");

            var item = basket.Items.FirstOrDefault(i => i.Id == basketItemId);
            if (item is null)
                throw new NotFoundException($"Item with ID {basketItemId} was not found in the basket.");

            if (quantity <= 0)
            {
                basket.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }

            await ReevaluateCouponDiscountAsync(basket);
            RecalculateTotals(basket);

            var saved = await _basketRepo.CreateOrUpdateBasketAsync(basket, CacheKeys.BasketExpiration);
            if (saved is null)
                throw new BadRequestException("Could not update basket. Please try again.");

            return _mapper.Map<BasketDto>(saved);
        }

        public async Task<BasketDto> ApplyCouponToBasketAsync(string couponCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
                throw new BadRequestException("Coupon code is required.");

            var userId = _currentUser.UserId;
            var key = CacheKeys.Basket(userId);
            var basket = await _basketRepo.GetBasketAsync(key);

            if (basket is null || !basket.Items.Any())
                throw new BadRequestException("Basket is empty. Add items to your basket before applying a coupon.");

            var coupon = await _couponRepo.GetByCodeAsync(couponCode.Trim());
            if (coupon is null)
                throw new NotFoundException($"Coupon with code '{couponCode}' was not found.");

            if (!coupon.IsActive)
                throw new BadRequestException("This coupon is inactive.");

            var now = DateTime.UtcNow;
            if (coupon.StartDate > now)
                throw new BadRequestException("This coupon is not active yet.");

            if (coupon.EndDate < now)
                throw new BadRequestException("This coupon has expired.");

            if (coupon.MaxUsage.HasValue && coupon.UsedCount >= coupon.MaxUsage.Value)
                throw new BadRequestException("This coupon has reached its maximum usage limit.");

            var subtotal = basket.Items.Sum(i => i.Price * i.Quantity);
            if (coupon.MinOrderAmount.HasValue && subtotal < coupon.MinOrderAmount.Value)
                throw new BadRequestException($"Minimum order subtotal of {coupon.MinOrderAmount.Value:C} is required to apply this coupon.");

            decimal discountAmount = coupon.Type == CouponType.Percentage
                ? Math.Round((subtotal * coupon.Value) / 100m, 2)
                : Math.Min(coupon.Value, subtotal);

            basket.CouponCode = coupon.Code.ToUpper();
            basket.CouponDiscount = discountAmount;

            RecalculateTotals(basket);

            var saved = await _basketRepo.CreateOrUpdateBasketAsync(basket, CacheKeys.BasketExpiration);
            if (saved is null)
                throw new BadRequestException("Could not save basket. Please try again.");

            return _mapper.Map<BasketDto>(saved);
        }

        public async Task<BasketDto> RemoveCouponFromBasketAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId;
            var key = CacheKeys.Basket(userId);
            var basket = await _basketRepo.GetBasketAsync(key);

            if (basket is null || !basket.Items.Any())
                throw new NotFoundException("Basket is empty or does not exist.");

            basket.CouponCode = null;
            basket.CouponDiscount = null;

            RecalculateTotals(basket);

            var saved = await _basketRepo.CreateOrUpdateBasketAsync(basket, CacheKeys.BasketExpiration);
            if (saved is null)
                throw new BadRequestException("Could not update basket. Please try again.");

            return _mapper.Map<BasketDto>(saved);
        }

        public async Task<bool> ClearBasketAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId;
            var key = CacheKeys.Basket(userId);
            return await _basketRepo.DeleteBasketAsync(key);
        }

   
        private async Task ReevaluateCouponDiscountAsync(CustomerBasket basket)
        {
            if (string.IsNullOrEmpty(basket.CouponCode)) return;

            var coupon = await _couponRepo.GetByCodeAsync(basket.CouponCode);
            if (coupon is null || !coupon.IsActive || coupon.EndDate < DateTime.UtcNow)
            {
                basket.CouponCode = null;
                basket.CouponDiscount = null;
                return;
            }

            var subtotal = basket.Items.Sum(i => i.Price * i.Quantity);
            if (coupon.MinOrderAmount.HasValue && subtotal < coupon.MinOrderAmount.Value)
            {
                basket.CouponCode = null;
                basket.CouponDiscount = null;
                return;
            }

            decimal discountAmount = coupon.Type == CouponType.Percentage
                ? Math.Round((subtotal * coupon.Value) / 100m, 2)
                : Math.Min(coupon.Value, subtotal);

            basket.CouponDiscount = discountAmount;
        }

        private static void RecalculateTotals(CustomerBasket basket)
        {
            var subtotal       = basket.Items.Sum(i => i.Price * i.Quantity);
            var shipping       = basket.ShippingCost    ?? 0m;
            var taxes          = basket.TaxesCost       ?? 0m;
            var offerDiscount  = basket.OfferDiscount   ?? 0m;
            var couponDiscount = basket.CouponDiscount  ?? 0m;

            var total = subtotal + shipping + taxes - offerDiscount - couponDiscount;
            basket.TotalPrice = total < 0 ? 0 : total;
        }
    }
}
