using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Buyly.Application.Constants;
using Buyly.Application.DTOs.Cart;
using Buyly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Buyly.API.Models;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace Buyly.API.Controllers
{
    [Authorize]
    public class CartController : BaseApiController
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [EnableRateLimiting(RateLimitPolicies.Strict)]
        [HttpPost("items")]
        [SwaggerOperation(
            Summary = "Add an item to the cart",
            Description = "Adds a product (or increments quantity) in the authenticated user’s cart. Respects strict rate limiting to prevent rapid-fire updates."
        )]
        [ProducesResponseType(typeof(ApiResponse<CartItemDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddItemToCart([FromBody] AddToCartDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItem = await _cartService.AddToCartAsync(userId!, dto);

            return Created(cartItem, "Item added to cart successfully.");
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get current cart",
            Description = "Returns line items, quantities, pricing, and totals for the signed-in user. Useful for rendering checkout or cart pages."
        )]
        [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _cartService.GetUserCartAsync(userId!);

            return Success(cart);
        }

        [EnableRateLimiting(RateLimitPolicies.Strict)]
        [HttpPut("items/{id:guid}")]
        [SwaggerOperation(
            Summary = "Update a cart item",
            Description = "Updates the quantity of an existing cart item by cart-item id. Rejects invalid quantities and enforces ownership."
        )]
        [ProducesResponseType(typeof(ApiResponse<CartItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCartItem([FromRoute] Guid id, [FromBody] UpdateCartItemDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var updatedItem = await _cartService.UpdateCartItemAsync(userId!, id, dto);

            return Success(updatedItem, "Cart Item Updated Successfully");
        }

        [EnableRateLimiting(RateLimitPolicies.Strict)]
        [HttpDelete("items/{id:guid}")]
        [SwaggerOperation(
            Summary = "Remove a cart item",
            Description = "Removes a specific cart item by its identifier, allowing the UI to drop individual lines."
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveCartItem([FromRoute] Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _cartService.RemoveCartItemAsync(userId!, id);

            if (!result)
            {
                return NotFound("Cart Item Not Found");
            }

            return Success("Cart Item Removed Successfully");
        }

        [EnableRateLimiting(RateLimitPolicies.Strict)]
        [HttpDelete]
        [SwaggerOperation(
            Summary = "Clear the entire cart",
            Description = "Deletes every line item for the authenticated user; typically used after checkout or explicit 'clear cart' action."
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _cartService.ClearCartAsync(userId!);

            return Success("Cart Cleared Successfully");
        }
    }
}
