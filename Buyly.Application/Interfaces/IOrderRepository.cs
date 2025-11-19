using Buyly.Domain.Entities;

namespace Buyly.Application.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
    Task<Order?> GetOrderWithDetailsAsync(Guid orderId);
}