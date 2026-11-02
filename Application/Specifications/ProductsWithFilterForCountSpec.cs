using Application.DTOs.Products;
using Domain.Entities;

namespace Application.Specifications
{
    public class ProductsWithFilterForCountSpec : BaseSpecification<Product>
    {
        public ProductsWithFilterForCountSpec(ProductQueryParams queryParams)
            : base(p =>
                (string.IsNullOrEmpty(queryParams.Search) || p.Name.ToLower().Contains(queryParams.Search)) &&
                (!queryParams.CategoryId.HasValue || p.CategoryId == queryParams.CategoryId.Value) &&
                (!queryParams.MinPrice.HasValue || p.Price >= queryParams.MinPrice.Value) &&
                (!queryParams.MaxPrice.HasValue || p.Price <= queryParams.MaxPrice.Value))
        {
        }
    }
}

