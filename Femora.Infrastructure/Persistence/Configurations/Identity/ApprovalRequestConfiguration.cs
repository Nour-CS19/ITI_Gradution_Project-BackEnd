

using Femora.Domain.Entities.Admin;
using Microsoft.EntityFrameworkCore;
//using Femora.Domain.Entities.Admin.ApprovalRequest;
//using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Persistence.Configurations.Identity
{

    public class ApprovalRequestConfiguration
        : IEntityTypeConfiguration<ApprovalRequest>
    {
        public void Configure(EntityTypeBuilder<ApprovalRequest> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.RequestedBy)
                .WithMany()
                .HasForeignKey(x => x.RequsterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReviwedBy)
                .WithMany()
                .HasForeignKey(x => x.AdminId)
                .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            builder.Property(x => x.ApprovalStatus)
                .HasConversion<string>();

            builder.Property(x => x.Type)
                .HasConversion<string>();

            builder.HasIndex(x => new
            {
                x.EntityId,
                x.Type
            })
            .HasFilter("[ApprovalStatus] = 'Pending'")
            .IsUnique();
        }
    }
}