using Application.Contracts.Repository;
using Application.Contracts.Services;
using Application.Contracts.Services.Common;
using Application.DTOs.Common;
using Application.DTOs.Products;
using Application.Exceptions;
using Application.Specifications;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IFileStorage _storage;

        public ProductService(IUnitOfWork uow, IMapper mapper, IFileStorage storage)
        {
            _uow = uow;
            _mapper = mapper;
            _storage = storage;
        }

        public async Task<PaginatedResponseDto<ProductDto>> GetAllProductsAsync(ProductQueryParams queryParams)
        {
            var productRepo = _uow.GetRepository<Product, int>();

            var spec = new ProductsWithCategorySpec(queryParams);
            var countSpec = new ProductsWithFilterForCountSpec(queryParams);

            var totalCount = await productRepo.CountAsync(countSpec);
            var products = await productRepo.GetAllAsync(spec);

            var dtoList = _mapper.Map<List<ProductDto>>(products);

            // Fetch active offers and apply them to products
            var activeOffers = (await _uow.GetRepository<Offer, int>().GetAllAsync(new ActiveOffersSpec())).ToList();
            ApplyOffersToDtos(dtoList, activeOffers);

            return new PaginatedResponseDto<ProductDto>
            {
                Data = dtoList,
                PageIndex = queryParams.PageIndex,
                PageSize = queryParams.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            var spec = new ProductsWithCategorySpec(productId);
            var product = await _uow.GetRepository<Product, int>().GetByIdAsync(productId, spec);
            if (product is null)
                throw new NotFoundException($"Product with ID {productId} was not found.");

            var dto = _mapper.Map<ProductDto>(product);

            // Fetch active offers and apply to this product
            var activeOffers = await _uow.GetRepository<Offer, int>().GetAllAsync(new ActiveOffersSpec());
            ApplyOfferToDto(dto, activeOffers);

            return dto;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            var product = _mapper.Map<Product>(createProductDto);
            await _uow.GetRepository<Product, int>().AddAsync(product);
            await _uow.SaveChangesAsync();

            var spec = new ProductsWithCategorySpec(product.Id);
            var createdProduct = await _uow.GetRepository<Product, int>().GetByIdAsync(product.Id, spec);
            var dto = _mapper.Map<ProductDto>(createdProduct ?? product);

            var activeOffers = await _uow.GetRepository<Offer, int>().GetAllAsync(new ActiveOffersSpec());
            ApplyOfferToDto(dto, activeOffers);

            return dto;
        }

        public async Task<ProductDto> UpdateProductAsync(int productId, UpdateProductDto updateProductDto)
        {
            var repository = _uow.GetRepository<Product, int>();
            var product = await repository.GetByIdAsync(productId);
            if (product is null)
                throw new NotFoundException($"Product with ID {productId} was not found.");

            _mapper.Map(updateProductDto, product);
            repository.Update(product);
            await _uow.SaveChangesAsync();

            var spec = new ProductsWithCategorySpec(product.Id);
            var updatedProduct = await repository.GetByIdAsync(product.Id, spec);
            var dto = _mapper.Map<ProductDto>(updatedProduct ?? product);

            var activeOffers = await _uow.GetRepository<Offer, int>().GetAllAsync(new ActiveOffersSpec());
            ApplyOfferToDto(dto, activeOffers);

            return dto;
        }

        public async Task DeleteProductAsync(int productId)
        {
            var repository = _uow.GetRepository<Product, int>();
            var product = await repository.GetByIdAsync(productId, new ProductsWithCategorySpec(productId));
            if (product is null)
                throw new NotFoundException($"Product with ID {productId} was not found.");

            var imageUrls = product.Images.Select(i => i.Url).ToList();

            repository.Delete(product);   // image rows are removed by cascade
            await _uow.SaveChangesAsync();

            foreach (var url in imageUrls)
                await _storage.DeleteAsync(url);
        }

        private static void ApplyOffersToDtos(IEnumerable<ProductDto> dtos, IEnumerable<Offer> activeOffers)
        {
            var offersList = activeOffers as IList<Offer> ?? activeOffers.ToList();
            foreach (var dto in dtos)
            {
                ApplyOfferToDto(dto, offersList);
            }
        }

        private static void ApplyOfferToDto(ProductDto dto, IEnumerable<Offer> activeOffers)
        {
            // Find offers matching either this specific product OR its category
            var applicableOffers = activeOffers.Where(o =>
                (o.ProductId.HasValue && o.ProductId.Value == dto.Id) ||
                (o.CategoryId.HasValue && o.CategoryId.Value == dto.CategoryId)
            ).ToList();

            // Select the offer with highest discount percentage
            var bestOffer = applicableOffers.OrderByDescending(o => o.DiscountPercentage).FirstOrDefault();

            if (bestOffer != null)
            {
                dto.DiscountPercentage = bestOffer.DiscountPercentage;
                dto.OfferTitle = bestOffer.Title;

                var discountAmount = Math.Round((dto.Price * bestOffer.DiscountPercentage) / 100m, 2);
                dto.PriceAfterDiscount = dto.Price - discountAmount;
            }
            else
            {
                dto.DiscountPercentage = null;
                dto.OfferTitle = null;
                dto.PriceAfterDiscount = null;
            }
        }
    }
}
