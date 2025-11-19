using Buyly.Application.Configurations;
using Buyly.Domain.Entities;
using Buyly.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Buyly.Infrastructure.HostedServices
{
    public class CartOrderCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CartOrderCleanupService> _logger;
        private readonly CleanupSettings _settings;

        public CartOrderCleanupService(
            IServiceScopeFactory scopeFactory,
            IOptions<CleanupSettings> options,
            ILogger<CartOrderCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _settings = options.Value ?? new CleanupSettings();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_settings.ExecutionIntervalMinutes <= 0)
            {
                _logger.LogInformation("CartOrderCleanupService disabled because ExecutionIntervalMinutes <= 0");
                return;
            }

            var delay = TimeSpan.FromMinutes(_settings.ExecutionIntervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunCleanupAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while running cart/order cleanup");
                }

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private async Task RunCleanupAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var now = DateTime.UtcNow;
            var changes = false;

            if (_settings.PendingOrderMaxAgeHours > 0)
            {
                var orderCutoff = now - TimeSpan.FromHours(_settings.PendingOrderMaxAgeHours);
                var staleOrders = await context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o =>
                        (o.Status == OrderStatus.PendingPayment || o.Status == OrderStatus.PaymentFailed) &&
                        o.OrderDate < orderCutoff)
                    .ToListAsync(cancellationToken);

                if (staleOrders.Any())
                {
                    await RestockProductsAsync(context, staleOrders.SelectMany(o => o.OrderItems), now, cancellationToken);

                    foreach (var order in staleOrders)
                    {
                        order.Status = OrderStatus.Cancelled;
                        order.UpdatedAt = now;
                    }

                    _logger.LogInformation("Cancelled {Count} stale pending orders older than {Hours}h", staleOrders.Count, _settings.PendingOrderMaxAgeHours);
                    changes = true;
                }
            }

            if (_settings.RestoredCartMaxAgeHours > 0)
            {
                var cartCutoff = now - TimeSpan.FromHours(_settings.RestoredCartMaxAgeHours);
                var staleCartItems = await context.CartItems
                    .Where(ci => (ci.UpdatedAt ?? ci.CreatedAt) < cartCutoff)
                    .ToListAsync(cancellationToken);

                if (staleCartItems.Any())
                {
                    context.CartItems.RemoveRange(staleCartItems);
                    _logger.LogInformation("Removed {Count} stale cart items older than {Hours}h", staleCartItems.Count, _settings.RestoredCartMaxAgeHours);
                    changes = true;
                }
            }

            if (changes)
            {
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        private static async Task RestockProductsAsync(
            AppDbContext context,
            IEnumerable<OrderItem> orderItems,
            DateTime timestamp,
            CancellationToken cancellationToken)
        {
            var grouped = orderItems
                .GroupBy(item => item.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(item => item.Quantity)
                })
                .ToList();

            if (!grouped.Any())
            {
                return;
            }

            var productIds = grouped.Select(g => g.ProductId).ToList();

            var products = await context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            foreach (var entry in grouped)
            {
                if (products.TryGetValue(entry.ProductId, out var product))
                {
                    product.Stock += entry.Quantity;
                    product.UpdatedAt = timestamp;
                }
            }
        }
    }
}

