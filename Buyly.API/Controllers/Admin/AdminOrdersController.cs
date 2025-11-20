using Buyly.API.Models;
using Buyly.Application.Constants;
using Buyly.Application.DTOs.Order;
using Buyly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Buyly.API.Controllers.Admin
{
    [Authorize(Roles = AuthorizationRoles.Admin)]
    [Route("api/admin/orders")]
    public class AdminOrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;

        public AdminOrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPut("{orderId:guid}/status")]
        [SwaggerOperation(
            Summary = "Admin: update order status",
            Description = "Allows staff to transition an order (Processing, Shipped, Delivered, Cancelled, etc.)."
        )]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusDto dto)
        {
            var result = await _orderService.UpdateOrderStatusAsync(orderId, dto.Status);
            if (!result)
                return NotFound(new ApiResponse<OrderDto> { Success = false, Message = "Order not found or Can't Update Order." });

            return Ok(new ApiResponse<OrderDto>
            {
                Success = true,
                Message = "Order status updated successfully."
            });
        }

        [HttpDelete("{orderId:guid}")]
        [SwaggerOperation(
            Summary = "Admin: delete order",
            Description = "Cancels/removes an order record. Use carefully—typically only for test or fraud orders."
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOrder(Guid orderId)
        {
            var result = await _orderService.DeleteOrderAsync(orderId);

            if (!result)
            {
                return NotFound("Order not found.");
            }

            return Success("Order deleted successfully.");
        }
    }
}

