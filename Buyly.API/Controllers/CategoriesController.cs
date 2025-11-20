using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Application.DTOs.Category;
using Buyly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Buyly.API.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Buyly.API.Controllers
{
    public class CategoriesController : BaseApiController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "List categories",
            Description = "Returns all product categories for building navigation menus or filters."
        )]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            
            return Success(categories, "Categories retrieved successfully.");
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get category by id",
            Description = "Fetches a single category by its GUID identifier."
        )]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
                return NotFound("Category not found.");

            return Success(category, "Category retrieved successfully.");
        }

    }
}
