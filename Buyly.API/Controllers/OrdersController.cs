using System.Security.Claims;
using Buyly.API.Models;
using Buyly.API.Models.Requests;
using Buyly.Application.Constants;
using Buyly.Application.DTOs.Order;
using Buyly.Application.DTOs.Payments;
using Buyly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Buyly.API.Controllers;

[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    
    public OrdersController(IOrderService orderService,
        ICartService cartService)
    {
        _orderService = orderService;
        _cartService = cartService;
    }
    
    [EnableRateLimiting(RateLimitPolicies.Strict)]
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<CheckoutResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest? request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new ApiResponse<OrderDto>{Success = false, Message = "User not authenticated."});
        var cart = await _cartService.GetUserCartAsync(userId);
        if (cart == null || !cart.Items.Any())
            return BadRequest(new ApiResponse<OrderDto>{Success = false, Message = "Cart is empty."});

        var dto = new CreateOrderDto
        {
            Items = cart.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.Price
            }).ToList(),
            Payment = request?.Payment ?? new PaymentInfoDto()
        };
        
        var result = await _orderService.CreateOrderAsync(userId, dto);

        await _cartService.ClearCartAsync(userId);
        
        return Ok(new ApiResponse<CheckoutResponseDto>
        {
            Success = true,
            Data = result,
            Message = "Order created. Complete payment to finalize your order."
        });
    }

    [EnableRateLimiting(RateLimitPolicies.Strict)]
    [HttpPost("{orderId:guid}/capture")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CaptureOrder(Guid orderId, [FromBody] CaptureOrderRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new ApiResponse<OrderDto>{Success = false, Message = "User not authenticated."});

        if (request == null || string.IsNullOrWhiteSpace(request.PayPalOrderId))
        {
            return BadRequest(new ApiResponse<OrderDto>{ Success = false, Message = "A valid PayPal order id is required." });
        }

        var order = await _orderService.CaptureOrderPaymentAsync(userId, orderId, request.PayPalOrderId);
        
        return Ok(new ApiResponse<OrderDto>
        {
            Success = true,
            Data = order,
            Message = "Payment captured successfully."
        });
    }
    
    [EnableRateLimiting(RateLimitPolicies.Strict)]
    [HttpPost("{orderId:guid}/retry")]
    [ProducesResponseType(typeof(ApiResponse<CheckoutResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RetryPayment(Guid orderId, [FromBody] RetryPaymentRequest? request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new ApiResponse<OrderDto>{Success = false, Message = "User not authenticated."});

        var response = await _orderService.RetryOrderPaymentAsync(userId, orderId, request?.Payment);

        return Ok(new ApiResponse<CheckoutResponseDto>
        {
            Success = true,
            Data = response,
            Message = "Payment retry initiated. Redirect the shopper to the approval link."
        });
    }
    
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid orderId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(new ApiResponse<OrderDto>{Success = false, Message = "User not authenticated."});

        var result = await _orderService.GetOrderByIdAsync(orderId);
        if (result == null)
            return NotFound(new ApiResponse<OrderDto>{Success = false, Message = "Order not found."});

        return Ok(new ApiResponse<OrderDto>
        {
            Success = true,
            Data = result,
            Message = "Order retrieved successfully."
        });
    }

    [HttpGet("user-orders")]
    [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserOrders()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(new ApiResponse<List<OrderDto>>{Success = false, Message = "User not authenticated."});

        var result = await _orderService.GetOrdersByUserIdAsync(userId);

        return Ok(new ApiResponse<List<OrderDto>>
        {
            Success = true,
            Data = result.ToList(),
            Message = "User orders retrieved successfully."
        });
    }

}
