using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Domain.Entities;

namespace Buyly.Application.Interfaces
{
    public interface ICartRepository : IGenericRepository<CartItem>
    {
        Task<IEnumerable<CartItem>> GetUserCartItemsAsync(string userId);
        Task<CartItem?> GetUserCartItemByProductIdAsync(string userId, Guid productId);
        Task ClearUserCartAsync(string userId);
        Task<IReadOnlyDictionary<Guid, CartItem>> GetUserCartItemsByIdsAsync(string userId, IEnumerable<Guid> cartItemIds);
    }
}