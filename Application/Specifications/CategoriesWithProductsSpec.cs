using Domain.Entities;

namespace Application.Specifications
{
    public class CategoriesWithProductsSpec : BaseSpecification<Category>
    {
        // 1. Get all categories including products count
        public CategoriesWithProductsSpec()
            : base()
        {
            AddInclude(c => c.Products);
            AddOrderBy(c => c.Name);
        }

        // 2. Get single category by ID with its products
        public CategoriesWithProductsSpec(int id)
            : base(c => c.Id == id)
        {
            AddInclude(c => c.Products);
        }
    }
}

