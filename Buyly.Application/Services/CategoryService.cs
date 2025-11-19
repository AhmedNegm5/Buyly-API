using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Application.DTOs.Category;
using Buyly.Application.Interfaces;
using Buyly.Domain.Entities;

namespace Buyly.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            var category = new Category
            {
                Name = createCategoryDto.Name,
                Description = createCategoryDto.Description,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.CommitAsync();

            return MapToCategoryDto(category);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);

            if (category == null)
                return false;

            _unitOfWork.Repository<Category>().Delete(category);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            return categories.Select(MapToCategoryDto).ToList();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null)
                return null;

            return MapToCategoryDto(category);
        }

        public async Task<bool> UpdateCategoryAsync(Guid id, UpdateCategoryDto updateCategoryDto)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null)
                return false;

            category.Name = updateCategoryDto.Name ?? category.Name;
            category.Description = updateCategoryDto.Description ?? category.Description;
            category.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Category>().Update(category);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private static CategoryDto MapToCategoryDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}