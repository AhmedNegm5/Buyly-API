using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Application.DTOs.Product;
using Buyly.Domain.Specifications;

namespace Buyly.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(ProductSpecParams specParams);
        Task<int> GetProductsCountAsync(ProductSpecParams specParams);
        Task<ProductDto?> GetProductByIdAsync(Guid id);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto);
        Task<bool> UpdateProductAsync(Guid id, UpdateProductDto dto);
        Task<bool> DeleteProductAsync(Guid id);
    }
}