using Application.DTOs.Offers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
    public interface IOfferService
    {
   
        Task<IEnumerable<OfferDto>> GetActiveOffersAsync(CancellationToken cancellationToken = default);


        Task<OfferDto?> GetActiveOfferForProductAsync(int productId, CancellationToken cancellationToken = default);

       
        Task<IEnumerable<OfferDto>> GetAllOffersAsync(CancellationToken cancellationToken = default);

   
        Task<OfferDto> GetOfferByIdAsync(int offerId, CancellationToken cancellationToken = default);

 
        Task<OfferDto> CreateOfferAsync(CreateOfferDto dto, CancellationToken cancellationToken = default);


        Task<OfferDto> UpdateOfferAsync(int offerId, UpdateOfferDto dto, CancellationToken cancellationToken = default);


        Task DeleteOfferAsync(int offerId, CancellationToken cancellationToken = default);
        Task<OfferDto> ToggleOfferStatusAsync(int offerId, CancellationToken cancellationToken = default);
    }
}

