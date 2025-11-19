using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Application.DTOs;
using Buyly.Application.DTOs.Product;
using Buyly.Application.Interfaces;
using Buyly.Domain.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Buyly.API.Models;

namespace Buyly.API.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] ProductSpecParams specParams)
        {
            var products = await _productService.GetAllProductsAsync(specParams);
            var count = await _productService.GetProductsCountAsync(specParams);

            var response = new
            {
                PageIndex = specParams.PageIndex,
                PageSize = specParams.PageSize,
                Count = count,
                Products = products
            };

            return Success(response, "Products retrieved successfully");
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound("Product not found");
            }

            return Success(product, "Product retrieved successfully");
        }

    }
}
