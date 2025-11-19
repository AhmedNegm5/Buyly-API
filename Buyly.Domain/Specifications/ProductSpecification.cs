using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Domain.Entities;

namespace Buyly.Domain.Specifications
{
    public class ProductSpecification : BaseSpecification<Product>
    {
        public ProductSpecification(ProductSpecParams specParams)
        {
            AddInclude(p => p.Category!);

            if (!string.IsNullOrEmpty(specParams.Category))
            {
                var categoryLower = specParams.Category.ToLower();
                AddCriteria(p => p.Category != null && p.Category.Name!.ToLower().Contains(categoryLower));
            }

            if (!string.IsNullOrEmpty(specParams.Search))
            {
                var searchLower = specParams.Search.ToLower();
                AddCriteria(p => p.Name!.ToLower().Contains(searchLower) ||
                (p.Description != null && p.Description.ToLower().Contains(searchLower)));
            }

            if (specParams.MinPrice.HasValue)
            {
                AddCriteria(p => p.Price >= specParams.MinPrice.Value);
            }

            if (specParams.MaxPrice.HasValue)
            {
                AddCriteria(p => p.Price <= specParams.MaxPrice.Value);
            }

            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch (specParams.Sort.ToLower())
                {
                    case "priceasc":
                        ApplyOrderBy(p => p.Price);
                        break;
                    case "pricedesc":
                        ApplyOrderByDescending(p => p.Price);
                        break;
                    case "nameasc":
                        ApplyOrderBy(p => p.Name!);
                        break;
                    case "namedesc":
                        ApplyOrderByDescending(p => p.Name!);
                        break;
                    case "newest":
                        ApplyOrderByDescending(p => p.CreatedAt);
                        break;
                    default:
                        ApplyOrderBy(p => p.Name!);
                        break;
                }
            }
            else
            {
                ApplyOrderBy(p => p.Name!);
            }

            ApplyPaging((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
            AsNoTracking();
        }

        public ProductSpecification(Guid id) : base()
        {
            AddCriteria(p => p.Id == id);
            AddInclude(p => p.Category!);
            AsNoTracking();
        }

    }
}