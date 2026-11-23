using Application.DTOs.Products;
using Domain.Entities;

namespace Application.Specifications
{
    public class ProductsWithCategorySpec : BaseSpecification<Product>
    {
        // 1. Used for listing products with filtering, searching, sorting and pagination
        public ProductsWithCategorySpec(ProductQueryParams queryParams)
            : base(p =>
                (string.IsNullOrEmpty(queryParams.Search) || p.Name.ToLower().Contains(queryParams.Search)) &&
                (!queryParams.CategoryId.HasValue || p.CategoryId == queryParams.CategoryId.Value) &&
                (!queryParams.MinPrice.HasValue || p.Price >= queryParams.MinPrice.Value) &&
                (!queryParams.MaxPrice.HasValue || p.Price <= queryParams.MaxPrice.Value))
        {
            AddInclude(p => p.Category);
            AddInclude(p => p.Images);

            switch (queryParams.Sort?.Trim().ToLower())
            {
                case "priceasc":
                case "price_asc":
                    AddOrderBy(p => p.Price);
                    break;
                case "pricedesc":
                case "price_desc":
                    AddOrderByDescending(p => p.Price);
                    break;
                case "rating":
                case "ratingdesc":
                case "rating_desc":
                    AddOrderByDescending(p => p.AverageRating);
                    break;
                case "namedesc":
                case "name_desc":
                    AddOrderByDescending(p => p.Name);
                    break;
                default:
                    AddOrderBy(p => p.Name);
                    break;
            }

            ApplyPaging((queryParams.PageIndex - 1) * queryParams.PageSize, queryParams.PageSize);
        }

        // 2. Used for getting a single product by ID with its Category
        public ProductsWithCategorySpec(int id)
            : base(p => p.Id == id)
        {
            AddInclude(p => p.Category);
            AddInclude(p => p.Images);
        }
    }
}
