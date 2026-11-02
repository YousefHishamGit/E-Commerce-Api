using Application.Contracts.Repository;
using Application.Contracts.Services;
using Application.DTOs.Offers;
using Application.Exceptions;
using Application.Specifications;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    public class OfferService : IOfferService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public OfferService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OfferDto>> GetActiveOffersAsync(CancellationToken cancellationToken = default)
        {
            var spec = new ActiveOffersSpec();
            var offers = await _uow.GetRepository<Offer, int>().GetAllAsync(spec);
            return _mapper.Map<IEnumerable<OfferDto>>(offers);
        }

        public async Task<OfferDto?> GetActiveOfferForProductAsync(int productId, CancellationToken cancellationToken = default)
        {
            var spec = new ActiveOffersSpec(productId);
            var offers = await _uow.GetRepository<Offer, int>().GetAllAsync(spec);
            var offer = offers.FirstOrDefault();

            return offer is null ? null : _mapper.Map<OfferDto>(offer);
        }

        public async Task<IEnumerable<OfferDto>> GetAllOffersAsync(CancellationToken cancellationToken = default)
        {
            var spec = new OffersWithDetailsSpec();
            var offers = await _uow.GetRepository<Offer, int>().GetAllAsync(spec);
            return _mapper.Map<IEnumerable<OfferDto>>(offers);
        }

        public async Task<OfferDto> GetOfferByIdAsync(int offerId, CancellationToken cancellationToken = default)
        {
            var spec = new OffersWithDetailsSpec(offerId);
            var offers = await _uow.GetRepository<Offer, int>().GetAllAsync(spec);
            var offer = offers.FirstOrDefault();

            if (offer is null)
                throw new NotFoundException($"Offer with ID {offerId} was not found.");

            return _mapper.Map<OfferDto>(offer);
        }

        public async Task<OfferDto> CreateOfferAsync(CreateOfferDto dto, CancellationToken cancellationToken = default)
        {
            if (dto.EndDate <= dto.StartDate)
                throw new BadRequestException("EndDate must be after StartDate.");

            if (dto.ProductId.HasValue)
            {
                var product = await _uow.GetRepository<Product, int>().GetByIdAsync(dto.ProductId.Value);
                if (product is null)
                    throw new NotFoundException($"Product with ID {dto.ProductId.Value} was not found.");
            }

            if (dto.CategoryId.HasValue)
            {
                var category = await _uow.GetRepository<Category, int>().GetByIdAsync(dto.CategoryId.Value);
                if (category is null)
                    throw new NotFoundException($"Category with ID {dto.CategoryId.Value} was not found.");
            }

            var offer = _mapper.Map<Offer>(dto);
            offer.IsActive = true;

            await _uow.GetRepository<Offer, int>().AddAsync(offer);
            await _uow.SaveChangesAsync();

            var spec = new OffersWithDetailsSpec(offer.Id);
            var createdOffers = await _uow.GetRepository<Offer, int>().GetAllAsync(spec);
            return _mapper.Map<OfferDto>(createdOffers.First());
        }

        public async Task<OfferDto> UpdateOfferAsync(int offerId, UpdateOfferDto dto, CancellationToken cancellationToken = default)
        {
            var offer = await _uow.GetRepository<Offer, int>().GetByIdAsync(offerId);
            if (offer is null)
                throw new NotFoundException($"Offer with ID {offerId} was not found.");

            var newStart = dto.StartDate ?? offer.StartDate;
            var newEnd = dto.EndDate ?? offer.EndDate;

            if (newEnd <= newStart)
                throw new BadRequestException("EndDate must be after StartDate.");

            _mapper.Map(dto, offer);
            _uow.GetRepository<Offer, int>().Update(offer);
            await _uow.SaveChangesAsync();

            var spec = new OffersWithDetailsSpec(offer.Id);
            var updatedOffers = await _uow.GetRepository<Offer, int>().GetAllAsync(spec);
            return _mapper.Map<OfferDto>(updatedOffers.First());
        }

        public async Task DeleteOfferAsync(int offerId, CancellationToken cancellationToken = default)
        {
            var offer = await _uow.GetRepository<Offer, int>().GetByIdAsync(offerId);
            if (offer is null)
                throw new NotFoundException($"Offer with ID {offerId} was not found.");

            _uow.GetRepository<Offer, int>().Delete(offer);
            await _uow.SaveChangesAsync();
        }

        public async Task<OfferDto> ToggleOfferStatusAsync(int offerId, CancellationToken cancellationToken = default)
        {
            var offer = await _uow.GetRepository<Offer, int>().GetByIdAsync(offerId);
            if (offer is null)
                throw new NotFoundException($"Offer with ID {offerId} was not found.");

            offer.IsActive = !offer.IsActive;
            _uow.GetRepository<Offer, int>().Update(offer);
            await _uow.SaveChangesAsync();

            var spec = new OffersWithDetailsSpec(offer.Id);
            var updatedOffers = await _uow.GetRepository<Offer, int>().GetAllAsync(spec);
            return _mapper.Map<OfferDto>(updatedOffers.First());
        }
    }
}

