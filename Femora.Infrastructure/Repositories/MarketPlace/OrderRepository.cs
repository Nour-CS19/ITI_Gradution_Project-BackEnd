using Femora.Application.Common.Interfaces.Repositories.MarketPlace;
using Femora.Domain.Entities.Marketplace;
using Femora.Domain.Enums;
using Femora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Repositoies.MarketPlace
{
    public class OrderRepository(AppDbContext context) : IOrderRepository
    {
        public async Task<Order?> GetByIdAsync(Guid orderId)
        {
            return await context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<Order>> GetByUserAsync(Guid userId)
        {
            return await context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetBySellerAsync(Guid sellerProfileId)
        {
            return await context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .Where(o => o.OrderItems.Any(
                    oi => oi.ProductVariant.Product.SellerProfileId == sellerProfileId))
                .ToListAsync();
        }

        public async Task<Order> CreateAsync(Order order)
        {
            await context.Orders.AddAsync(order);

            await context.SaveChangesAsync();

            return order;
        }

        public async Task UpdateStatusAsync(Guid orderId, OrderStatus status)
        {
            var order = await context.Orders
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order is null)
                throw new Exception("Order not found");

            order.Status = status;

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid orderId)
        {
            var order = await context.Orders
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order is null)
                return;

            context.Orders.Remove(order);

            await context.SaveChangesAsync();
        }
    }
}
