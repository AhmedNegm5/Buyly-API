using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Buyly.Application.DTOs.Cart;
using Buyly.Domain.Entities;

namespace Buyly.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartItemDto> AddToCartAsync(string userId, AddToCartDto dto);
        Task<CartDto?> GetUserCartAsync(string userId);
        Task<CartItemDto> UpdateCartItemAsync(string userId, Guid cartItemId, UpdateCartItemDto dto);
        Task<bool> RemoveCartItemAsync(string userId, Guid cartItemId);
        Task ClearCartAsync(string userId);
        Task RestoreCartFromOrderAsync(string userId, IEnumerable<OrderItem> orderItems);
    }
}