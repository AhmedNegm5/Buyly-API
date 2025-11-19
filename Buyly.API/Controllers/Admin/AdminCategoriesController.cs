using Buyly.API.Models;
using Buyly.Application.Constants;
using Buyly.Application.DTOs.Category;
using Buyly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buyly.API.Controllers.Admin
{
    [Authorize(Roles = AuthorizationRoles.Admin)]
    [Route("api/admin/categories")]
    public class AdminCategoriesController : BaseApiController
    {
        private readonly ICategoryService _categoryService;

        public AdminCategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            var category = await _categoryService.CreateCategoryAsync(createCategoryDto);
            return Success(category, "Category created successfully.");
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);

            if (!updatedCategory)
                return NotFound("Category not found.");

            return Success("Category updated successfully.");
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result)
                return NotFound("Category not found.");

            return Success("Category deleted successfully.");
        }
    }
}

