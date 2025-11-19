using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Application.Interfaces;
using Buyly.Domain.Entities;
using Buyly.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Buyly.Infrastructure.Repositories
{
    public class CartRepository : GenericRepository<CartItem>, ICartRepository
    {
        public CartRepository(AppDbContext context) : base(context)
        {
        }
        public async Task ClearUserCartAsync(string userId)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
        }

        public async Task<CartItem?> GetUserCartItemByProductIdAsync(string userId, Guid productId)
        {
            return await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);
        }

        public async Task<IEnumerable<CartItem>> GetUserCartItemsAsync(string userId)
        {
            return await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .ToListAsync();
        }

        public async Task<IReadOnlyDictionary<Guid, CartItem>> GetUserCartItemsByIdsAsync(string userId, IEnumerable<Guid> cartItemIds)
        {
            if (cartItemIds == null) throw new ArgumentNullException(nameof(cartItemIds));
            var ids = cartItemIds.Distinct().ToList();
            if (!ids.Any())
            {
                return new Dictionary<Guid, CartItem>();
            }

            var items = await _context.CartItems
                .Where(ci => ci.UserId == userId && ids.Contains(ci.Id))
                .ToListAsync();

            return items.ToDictionary(ci => ci.Id);
        }
    }
}