using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Application.DTOs.Product;
using Buyly.Application.Exceptions;
using Buyly.Application.Interfaces;
using Buyly.Domain.Entities;
using Buyly.Domain.Specifications;

namespace Buyly.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductRepository _productRepository;
        public ProductService(IUnitOfWork unitOfWork,
            IProductRepository productRepository)
        {
            _unitOfWork = unitOfWork;
            _productRepository = productRepository;
        }
        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {

            var categoryExists = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.CategoryId);


            if (categoryExists == null)
            {
                throw new NotFoundException("Category", dto.CategoryId.ToString());
            }

            ValidateProductData(dto);

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId,
                ImageUrl = dto.ImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.CommitAsync();
            product.Category = categoryExists;

            return MapToProductDto(product);
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);

            if (product == null)
                return false;

            _unitOfWork.Repository<Product>().Delete(product);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(ProductSpecParams specParams)
        {
            var spec = new ProductSpecification(specParams);
            return await _productRepository.ListAsync(spec, p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CategoryName = p.Category != null ? p.Category.Name : null,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });
        }

        public async Task<int> GetProductsCountAsync(ProductSpecParams specParams)
        {
            var spec = new ProductWithFiltersForCountSpecification(specParams);
            return await _productRepository.CountAsync(spec);
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid id)
        {
            var spec = new ProductSpecification(id);
            var results = await _productRepository.ListAsync(spec, p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CategoryName = p.Category != null ? p.Category.Name : null,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });

            return results.FirstOrDefault();
        }

        public async Task<bool> UpdateProductAsync(Guid id, UpdateProductDto dto)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null)
                return false;

            if (dto.CategoryId.HasValue)
            {
                var categoryExists = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.CategoryId.Value);
                if (categoryExists == null)
                {
                    throw new NotFoundException("Category", dto.CategoryId.Value.ToString());
                }
                product.CategoryId = dto.CategoryId.Value;
            }

            product.Name = dto.Name ?? product.Name;
            product.Description = dto.Description ?? product.Description;
            product.Price = dto.Price ?? product.Price;
            product.Stock = dto.Stock ?? product.Stock;
            product.UpdatedAt = DateTime.UtcNow;
            product.ImageUrl = dto.ImageUrl ?? product.ImageUrl;

            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private static void ValidateProductData(CreateProductDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                errors.Add("Product name cannot be empty.");
            }

            if (dto.Price <= 0)
            {
                errors.Add("Product price must be greater than zero.");
            }

            if (dto.Stock < 0)
            {
                errors.Add("Product stock cannot be negative.");
            }

            if (errors.Any())
            {
                throw new ValidationException(errors.ToArray());
            }
        }

        private static ProductDto MapToProductDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                CategoryName = product.Category?.Name,
                ImageUrl = product.ImageUrl
            };
        }
    }
}