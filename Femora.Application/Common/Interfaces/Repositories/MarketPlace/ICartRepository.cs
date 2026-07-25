using Femora.Domain.Entities.Marketplace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Common.Interfaces.Repositories.MarketPlace
{
    public interface ICartRepository
    {
        Task<Cart> GetByUserIdAsync(Guid userId);

        Task AddItemAsync(Guid userId, Guid productVariantId, int quantity);

        Task UpdateItemQuantityAsync(Guid cartItemId, int quantity);

        Task RemoveItemAsync(Guid cartItemId);

        Task ClearCartAsync(Guid userId);
    }
}
