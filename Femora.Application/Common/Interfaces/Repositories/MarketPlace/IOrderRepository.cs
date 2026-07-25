using Femora.Domain.Entities.Marketplace;
using Femora.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Common.Interfaces.Repositories.MarketPlace
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid orderId);

        Task<IEnumerable<Order>> GetByUserAsync(Guid userId);

        Task<IEnumerable<Order>> GetBySellerAsync(Guid sellerProfileId);

        Task<Order> CreateAsync(Order order);

        Task UpdateStatusAsync(Guid orderId, OrderStatus status);

        Task DeleteAsync(Guid orderId);




    }
}
