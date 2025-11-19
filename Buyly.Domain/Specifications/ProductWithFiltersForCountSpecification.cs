using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Domain.Entities;

namespace Buyly.Domain.Specifications
{
    public class ProductWithFiltersForCountSpecification
        : BaseSpecification<Product>
    {
        public ProductWithFiltersForCountSpecification(ProductSpecParams specParams)
        {
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

            AsNoTracking();
        }
    }
}