using Buyly.Application.Interfaces;
using Buyly.Domain.Entities;
using Buyly.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Buyly.Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order> , IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderWithDetailsAsync(Guid orderId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }
}