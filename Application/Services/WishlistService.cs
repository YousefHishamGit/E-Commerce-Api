using Application.Contracts.Repository;
using Application.Contracts.Services;
using Application.Contracts.Services.Common;
using Application.DTOs.Wishlist;
using Application.Exceptions;
using Application.Specifications;
using AutoMapper;
using Domain.Entities;

namespace Application.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;

        public WishlistService(
            IUnitOfWork uow,
            IMapper mapper,
            ICurrentUser currentUser)
        {
            _uow = uow;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<WishlistDto> GetUserWishlistAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId;
            var spec = new WishlistWithItemsSpec(userId);
            var wishlists = await _uow.GetRepository<Wishlist, int>().GetAllAsync(spec);
            var wishlist = wishlists.FirstOrDefault();

            if (wishlist is null)
            {
                return new WishlistDto
                {
                    UserId = userId,
                    Items = new List<WishlistItemDto>()
                };
            }

            return _mapper.Map<WishlistDto>(wishlist);
        }

        public async Task<WishlistDto> AddProductToWishlistAsync(int productId, CancellationToken cancellationToken = default)
        {
            if (productId <= 0)
                throw new BadRequestException("Invalid product ID.");

            // 1. Verify the product exists
            var product = await _uow.GetRepository<Product, int>().GetByIdAsync(productId);
            if (product is null)
                throw new NotFoundException($"Product with ID {productId} was not found.");

            var userId = _currentUser.UserId;
            var spec = new WishlistWithItemsSpec(userId);
            var wishlists = await _uow.GetRepository<Wishlist, int>().GetAllAsync(spec);
            var wishlist = wishlists.FirstOrDefault();

            // 2. Create wishlist if user doesn't have one yet
            if (wishlist is null)
            {
                wishlist = new Wishlist
                {
                    UserId = userId
                };
                await _uow.GetRepository<Wishlist, int>().AddAsync(wishlist);
                await _uow.SaveChangesAsync();
            }

            // 3. Prevent duplicate products in the wishlist
            if (wishlist.Items.Any(i => i.ProductId == productId))
                throw new ConflictException("Product is already in your wishlist.");

            // 4. Add the item
            var wishlistItem = new WishlistItem
            {
                WishlistId = wishlist.Id,
                ProductId = productId
            };

            await _uow.GetRepository<WishlistItem, int>().AddAsync(wishlistItem);
            await _uow.SaveChangesAsync();

            // 5. Reload with full product details
            var updatedWishlists = await _uow.GetRepository<Wishlist, int>().GetAllAsync(spec);
            return _mapper.Map<WishlistDto>(updatedWishlists.First());
        }

        public async Task<WishlistDto> RemoveProductFromWishlistAsync(int productId, CancellationToken cancellationToken = default)
        {
            if (productId <= 0)
                throw new BadRequestException("Invalid product ID.");

            var userId = _currentUser.UserId;
            var spec = new WishlistWithItemsSpec(userId);
            var wishlists = await _uow.GetRepository<Wishlist, int>().GetAllAsync(spec);
            var wishlist = wishlists.FirstOrDefault();

            if (wishlist is null || !wishlist.Items.Any())
                throw new NotFoundException("Wishlist is empty or does not exist.");

            var item = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item is null)
                throw new NotFoundException($"Product with ID {productId} is not in your wishlist.");

            _uow.GetRepository<WishlistItem, int>().Delete(item);
            await _uow.SaveChangesAsync();

            wishlist.Items.Remove(item);
            return _mapper.Map<WishlistDto>(wishlist);
        }

        public async Task<bool> IsProductInWishlistAsync(int productId, CancellationToken cancellationToken = default)
        {
            if (productId <= 0)
                return false;

            var userId = _currentUser.UserId;
            var spec = new WishlistWithItemsSpec(userId);
            var wishlists = await _uow.GetRepository<Wishlist, int>().GetAllAsync(spec);
            var wishlist = wishlists.FirstOrDefault();

            return wishlist?.Items.Any(i => i.ProductId == productId) ?? false;
        }

        public async Task<bool> ClearWishlistAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId;
            var spec = new WishlistWithItemsSpec(userId);
            var wishlists = await _uow.GetRepository<Wishlist, int>().GetAllAsync(spec);
            var wishlist = wishlists.FirstOrDefault();

            if (wishlist is null || !wishlist.Items.Any())
                return true;

            var itemRepo = _uow.GetRepository<WishlistItem, int>();
            foreach (var item in wishlist.Items.ToList())
            {
                itemRepo.Delete(item);
            }

            await _uow.SaveChangesAsync();
            return true;
        }
    }
}

