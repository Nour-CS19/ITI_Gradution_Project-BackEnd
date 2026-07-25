using Femora.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Data.Configurations.Marketplace
{
    public class SellerEarningConfiguration : IEntityTypeConfiguration<SellerEarning>
    {
        public void Configure(EntityTypeBuilder<SellerEarning> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SellerProfileId)
                .IsRequired();

            builder.Property(x => x.OrderItemId)
                   .IsRequired();

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.EarnedAt)
                .IsRequired();

            builder.Property(x => x.PlatformFee)
                .HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.OrderItem)
                .WithMany()
                .HasForeignKey(x => x.OrderItemId);
        }
    }
}
