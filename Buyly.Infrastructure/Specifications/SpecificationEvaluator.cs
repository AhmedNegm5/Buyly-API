using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Domain.Entities;
using Buyly.Domain.Specifications;

using Microsoft.EntityFrameworkCore;

namespace Buyly.Infrastructure.Specifications
{
    public class SpecificationEvaluator<T> where T : BaseEntity
    {
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
        {
            var query = inputQuery;

            // Apply criteria
            if (spec.Criteria.Count > 0)
            {
                foreach (var criteria in spec.Criteria)
                {
                    query = query.Where(criteria);
                }
            }


            // Apply ordering
            if (spec.OrderBy != null)
            {
                query = query.OrderBy(spec.OrderBy);
            }
            else if (spec.OrderByDescending != null)
            {
                query = query.OrderByDescending(spec.OrderByDescending);
            }

            // Apply paging
            if (spec.IsPagingEnabled)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }

            query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

            if (!spec.IsTrackingEnabled)
            {
                query = query.AsNoTracking();
            }

            return query;
        }
    }
}