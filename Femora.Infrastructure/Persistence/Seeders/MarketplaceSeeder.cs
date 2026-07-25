using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Entities.Marketplace;
using Femora.Domain.Enums;
using Femora.Infrastructure.Data;

namespace Femora.Infrastructure.Persistence.Seeders
{
    internal static class MarketplaceSeeder
    {
        private static readonly Dictionary<string, string[]> ProductsData = new()
        {
            {
                "Crochet Products",
                new[]
                {
                    "Crochet Teddy Bear|Adorable handmade crochet teddy bear with soft polyester fiber filling",
                    "Crochet Handbag|Beautiful shoulder bag made with premium yarn in multiple colors",
                    "Crochet Baby Shoes|Soft and warm crochet booties perfect for babies",
                    "Crochet Flowers|Decorative crochet flowers for arrangements and gifts",
                    "Crochet Baby Blanket|Cozy baby blanket made with hypoallergenic yarn"
                }
            },
            {
                "Knitted Clothes",
                new[]
                {
                    "Hand-Knit Sweater|Warm and comfortable sweater knitted with merino wool",
                    "Knitted Scarf|Beautiful scarf in various colors and patterns",
                    "Hand-Knit Cardigan|Elegant cardigan perfect for any occasion",
                    "Knitted Baby Hat|Soft and cute baby hat knitted with gentle yarn",
                    "Hand-Knit Socks|Cozy socks with various designs and colors"
                }
            },
            {
                "Handmade Bags",
                new[]
                {
                    "Leather Tote Bag|Durable leather bag perfect for daily use",
                    "Canvas Shoulder Bag|Spacious bag with embroidered details",
                    "Woven Market Bag|Traditional woven bag from natural fibers",
                    "Embroidered Clutch|Elegant clutch with traditional embroidery",
                    "Beaded Evening Bag|Beautiful bag perfect for formal events"
                }
            },
            {
                "Home Decor",
                new[]
                {
                    "Macramé Wall Hanging|Beautiful wall art made with natural cord",
                    "Embroidered Wall Tapestry|Traditional embroidery wall decoration",
                    "Ceramic Wall Tiles|Handmade decorative tiles for walls",
                    "Crochet Doily|Vintage-style handmade doily",
                    "Plant Macramé Hanger|Decorative hanger for indoor plants"
                }
            },
            {
                "Candles",
                new[]
                {
                    "Vanilla Soy Candle|Handmade soy candle with pure vanilla extract",
                    "Lavender Scented Candle|Calming lavender-scented candle",
                    "Rose Garden Candle|Floral scented candle with essential oils",
                    "Jasmine Night Candle|Exotic jasmine fragrance in handmade candle",
                    "Gift Candle Box Set|Set of three assorted scented candles"
                }
            },
            {
                "Natural Soap",
                new[]
                {
                    "Olive Oil Soap|Pure olive oil soap made using cold process",
                    "Lavender Honey Soap|Relaxing soap with lavender and raw honey",
                    "Rose Clay Soap|Gentle soap with rose clay for sensitive skin",
                    "Charcoal Detox Soap|Activated charcoal soap for deep cleansing",
                    "Herbal Garden Soap|Mixed herbal soap with natural botanicals"
                }
            },
            {
                "Jewelry",
                new[]
                {
                    "Beaded Bracelet|Colorful bracelet made with semi-precious stones",
                    "Silver Wire Necklace|Elegant necklace with silver wire wrapped details",
                    "Pearl Earrings|Classic pearl earrings with 925 silver hooks",
                    "Gemstone Ring|Beautiful ring with natural gemstone",
                    "Handmade Anklet|Delicate anklet with beads and charms"
                }
            },
            {
                "Resin Products",
                new[]
                {
                    "Resin Coaster Set|Set of four decorative resin coasters",
                    "Resin Bookmark|Beautiful bookmark with dried flowers",
                    "Resin Keychain|Small keychain with embedded design",
                    "Resin Tray|Functional decorative tray for jewelry storage",
                    "Resin Wall Art|Abstract resin art piece for walls"
                }
            },
            {
                "Pottery",
                new[]
                {
                    "Hand-Thrown Mug|Ceramic mug handmade on pottery wheel",
                    "Ceramic Vase|Decorative vase with glazed finish",
                    "Pottery Plate Set|Set of four handmade ceramic plates",
                    "Clay Flower Pot|Decorative pot for indoor plants",
                    "Ceramic Bowl|Handmade serving bowl with unique pattern"
                }
            },
            {
                "Craft Kits",
                new[]
                {
                    "Crochet Starter Kit|Complete kit for beginners to learn crochet",
                    "Soap Making Kit|DIY kit with all materials for soap making",
                    "Resin Art Kit|Beginner-friendly resin art project kit",
                    "Jewelry Making Kit|Complete jewelry making tools and materials",
                    "Embroidery Kit|All-in-one embroidery project kit with designs"
                }
            }
        };

        private static readonly string[] SizeVariants = new[] { "Small", "Medium", "Large" };
        private static readonly string[] ColorVariants = new[] { "Red", "Blue", "Green", "Yellow", "Pink", "Purple", "Black", "White", "Brown", "Gray" };

        public static async Task SeedAsync(AppDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            // Get seller profiles
            var sellerProfiles = await context.SellerProfiles
                .Include(sp => sp.User)
                .Where(sp => sp.Status == VerificationStatus.Approved)
                .ToListAsync();

            if (sellerProfiles.Count == 0)
                return;

            // Get product categories
            var categories = await context.ProductCategories.ToListAsync();
            if (categories.Count == 0)
                return;

            // Get trainee profiles for orders
            var traineeProfiles = await context.TraineeProfiles.ToListAsync();

            int sellerIndex = 0;
            int productCount = 0;

            // Create products
            foreach (var category in categories)
            {
                if (!ProductsData.TryGetValue(category.Name, out var productsInCategory))
                    continue;

                foreach (var productData in productsInCategory)
                {
                    var parts = productData.Split('|');
                    var productName = parts[0];
                    var productDescription = parts[1];

                    // Check if product already exists
                    if (context.Products.Any(p => p.Name == productName))
                        continue;

                    var seller = sellerProfiles[sellerIndex % sellerProfiles.Count];

                    var product = new Product
                    {
                        SellerProfileId = seller.Id,
                        ProductCategoryId = category.Id,
                        Name = productName,
                        Description = productDescription,
                        IsPuplished = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Products.Add(product);
                    await context.SaveChangesAsync();

                    // Add variants
                    var variantCount = 2 + (productCount % 3);
                    for (int v = 0; v < variantCount; v++)
                    {
                        var variant = new ProductVariant
                        {
                            ProductId = product.Id,
                            Name = $"{SizeVariants[v % SizeVariants.Length]} - {ColorVariants[(productCount + v) % ColorVariants.Length]}",
                            Price = 9.99m + (productCount * 5m) + (v * 2m),
                            StockQuantity = 50 + (productCount * 10)
                        };
                        context.ProductVariants.Add(variant);
                    }
                    await context.SaveChangesAsync();

                    sellerIndex++;
                    productCount++;
                }
            }

            // Create orders
            var orderStatuses = new[] { OrderStatus.Pending, OrderStatus.Processing };
            var products = await context.Products
                .Include(p => p.ProductVariants)
                .ToListAsync();

            foreach (var trainee in traineeProfiles)
            {
                var user = await userManager.FindByIdAsync(trainee.UserId.ToString());
                if (user == null)
                    continue;

                // Skip admin user — admin should not have marketplace orders
                if (user.Email.Equals("admin@test.com", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Create 2-3 orders per trainee
                var orderCount = 2 + (trainee.Id.GetHashCode() % 2);
                for (int o = 0; o < orderCount; o++)
                {
                    // Check if order already exists
                    if (context.Orders.Any(ord => ord.UserId == user.Id && ord.CreatedAt.Day == DateTime.UtcNow.Day))
                        continue;

                    var order = new Order
                    {
                        UserId = user.Id,
                        Status = orderStatuses[o % orderStatuses.Length],
                        TotalAmount = 0m,
                        CreatedAt = DateTime.UtcNow.AddDays(-(trainee.Id.GetHashCode() % 30))
                    };
                    context.Orders.Add(order);
                    await context.SaveChangesAsync();

                    // Add order items
                    var productsForOrder = products
                        .OrderBy(x => Guid.NewGuid())
                        .Take(1 + (o % 3))
                        .ToList();

                    decimal orderTotal = 0;
                    foreach (var prod in productsForOrder)
                    {
                        var variant = prod.ProductVariants.FirstOrDefault();
                        if (variant == null)
                            continue;

                        var quantity = 1 + (o % 3);
                        var itemTotal = variant.Price * quantity;
                        orderTotal += itemTotal;

                        var orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ProductVariantId = variant.Id,
                            Quantity = quantity,
                            UnitPrice = variant.Price
                        };
                        context.OrderItems.Add(orderItem);
                    }

                    order.TotalAmount = orderTotal;

                    // Add payment if order is paid
                    if (order.Status == OrderStatus.Processing)
                    {
                        var payment = new Payment
                        {
                            UserId = user.Id,
                            OrderId = order.Id,
                            Amount = orderTotal,
                            PaymentMethod = "Credit Card",
                            PaymentStatus = "Completed",
                            TransactionReference = $"TXN-{Guid.NewGuid().ToString().Substring(0, 8)}",
                            PaidAt = DateTime.UtcNow.AddDays(-(trainee.Id.GetHashCode() % 20))
                        };
                        context.Payments.Add(payment);
                    }

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
