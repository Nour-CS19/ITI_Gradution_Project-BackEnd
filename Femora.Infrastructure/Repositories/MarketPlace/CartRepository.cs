using Femora.Application.Common.Interfaces.Repositories.MarketPlace;
using Femora.Domain.Entities.Marketplace;
using Femora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Repositoies.MarketPlace
{
    public class CartRepository(AppDbContext context) : ICartRepository
    {
        public async Task<Cart> GetByUserIdAsync(Guid userId)
        {
            var cart = await context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is null)
            {
                cart = new Cart
                {
                    UserId = userId
                };

                context.Carts.Add(cart);
                await context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task AddItemAsync(Guid userId, Guid productVariantId, int quantity)
        {
            var cart = await GetByUserIdAsync(userId);

            var item = cart.Items
                .FirstOrDefault(x => x.ProductVariantId == productVariantId);

            if (item is not null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductVariantId = productVariantId,
                    Quantity = quantity
                });
            }

            await context.SaveChangesAsync();
        }

        public async Task UpdateItemQuantityAsync(Guid cartItemId, int quantity)
        {
            var item = await context.CartItems
                .FirstOrDefaultAsync(x => x.Id == cartItemId);

            if (item is null)
                throw new Exception("Cart item not found");

            item.Quantity = quantity;

            await context.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(Guid cartItemId)
        {
            var item = await context.CartItems
                .FirstOrDefaultAsync(x => x.Id == cartItemId);

            if (item is null)
                return;

            context.CartItems.Remove(item);

            await context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(Guid userId)
        {
            var cart = await context.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart is null)
                return;

            context.CartItems.RemoveRange(cart.Items);

            await context.SaveChangesAsync();
        }
    }
}
