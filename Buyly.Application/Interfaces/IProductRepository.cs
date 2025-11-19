using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Buyly.Domain.Entities;
using Buyly.Domain.Specifications;

namespace Buyly.Application.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<Product?> GetProductWithCategoryAsync(Guid id);
        Task<IEnumerable<Product>> GetAllProductsWithCategorysAsync();
        Task<IReadOnlyDictionary<Guid, Product>> GetProductsByIdsAsync(IEnumerable<Guid> ids);
        Task<IEnumerable<TResult>> ListAsync<TResult>(ISpecification<Product> spec, Expression<Func<Product, TResult>> selector);
    }
}