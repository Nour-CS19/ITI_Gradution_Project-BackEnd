using Femora.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Data.Configurations.Identity;
public class AIConversation : IEntityTypeConfiguration<InstructorCredential>
{
    public void Configure(EntityTypeBuilder<InstructorCredential> builder)
    {
        builder.HasKey(ic => ic.Id);

        // -- PROPERTIES --
        builder.Property(ic => ic.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(ic => ic.Title)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(ic => ic.UploadedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // -- RELATIONSHIPS --
        builder.HasOne(ic => ic.InstructorProfile)
            .WithMany(ip => ip.Credentials)
            .HasForeignKey(ic => ic.InstructorProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
