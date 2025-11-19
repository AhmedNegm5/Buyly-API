using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Buyly.Application.Interfaces;
using Buyly.Domain.Entities;
using Buyly.Domain.Specifications;
using Buyly.Infrastructure.Data;
using Buyly.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Buyly.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }
        
        public async Task<IEnumerable<Product>> GetAllProductsWithCategorysAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<Product?> GetProductWithCategoryAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IReadOnlyDictionary<Guid, Product>> GetProductsByIdsAsync(IEnumerable<Guid> ids)
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids));
            var idList = ids.Distinct().ToList();
            if (!idList.Any())
            {
                return new Dictionary<Guid, Product>();
            }

            var products = await _context.Products
                .Where(p => idList.Contains(p.Id))
                .ToListAsync();

            return products.ToDictionary(p => p.Id);
        }

        public async Task<IEnumerable<TResult>> ListAsync<TResult>(ISpecification<Product> spec, Expression<Func<Product, TResult>> selector)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var queryable = SpecificationEvaluator<Product>.GetQuery(_context.Products.AsQueryable(), spec);
            return await queryable.Select(selector).ToListAsync();
        }
    }
}