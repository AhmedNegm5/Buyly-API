using Buyly.API.Models;
using Buyly.Application.Constants;
using Buyly.Application.DTOs.Order;
using Buyly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

