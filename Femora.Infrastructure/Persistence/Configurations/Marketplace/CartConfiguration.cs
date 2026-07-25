using Femora.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Femora.Domain.Entities.Identity;

namespace Femora.Infrastructure.Data.Configurations.Marketplace
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            // A user should only ever have one cart. Without this, two concurrent
            // "first ever cart load" requests could each successfully insert a Cart row
            // for the same user (see GetCartQueryHandler for the app-level guard, which
            // only works if the DB actually rejects the second insert).
            builder.HasIndex(x => x.UserId)
                .IsUnique();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.RowVersion)
                .IsRowVersion();

            builder.HasMany(x => x.Items)
                .WithOne(x => x.Cart)
                .HasForeignKey(x => x.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
