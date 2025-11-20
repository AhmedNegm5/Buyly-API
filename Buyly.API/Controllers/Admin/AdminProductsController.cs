using Buyly.API.Models;
using Buyly.Application.Constants;
using Buyly.Application.DTOs.Product;
using Buyly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Buyly.API.Controllers.Admin
{
    [Authorize(Roles = AuthorizationRoles.Admin)]
    [Route("api/admin/products")]
    public class AdminProductsController : BaseApiController
    {
        private readonly IProductService _productService;

        public AdminProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Admin: create product",
            Description = "Creates a catalog product with inventory, pricing, description, and category assignment."
        )]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var createdProduct = await _productService.CreateProductAsync(dto);
            return Created(createdProduct, "Product created successfully");
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Admin: update product",
            Description = "Updates product metadata (name, description, price, inventory, category)."
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
        {
            var updatedProduct = await _productService.UpdateProductAsync(id, dto);

            if (!updatedProduct)
            {
                return NotFound("Product not found");
            }

            return Success("Product updated successfully");
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Admin: delete product",
            Description = "Permanently removes a product by its identifier."
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _productService.DeleteProductAsync(id);

            if (!deleted)
            {
                return NotFound("Product not found");
            }

            return Success("Product deleted successfully");
        }
    }
}

