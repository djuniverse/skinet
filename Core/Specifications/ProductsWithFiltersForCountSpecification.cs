using Core.Entities;

namespace Core.Specifications;

public class ProductsWithFiltersForCountSpecification : BaseSpecification<Product>
{
    public ProductsWithFiltersForCountSpecification(ProductSpecParams productParams)
        : base(x =>
            (string.IsNullOrEmpty(productParams.Search) || x.Name.ToLower().Contains(productParams.Search)) &&
            (!productParams.BrandId.HasValue || x.ProductBrandId == productParams.BrandId) &&
            (!productParams.TypeId.HasValue ||
             x.ProductTypeId ==
             productParams
                 .TypeId)) // || to jest "or else" czyli jeżeli ma wartośc (!brandId.HasValue == false) to wykonaj przypisanie
    {
    }
}