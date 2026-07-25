using Femora.Domain.Common;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Entities.Marketplace;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Femora.Domain.Entities.Marketplace;

/// <summary>
/// Tracks a trainee's preferred marketplace product categories,
/// set during onboarding or from their profile, used to power
/// AI product recommendations.
/// </summary>
public class TraineePreferredProductCategory : BaseEntity
{
    [Required]
    public Guid TraineeProfileId { get; set; }

    [ForeignKey(nameof(TraineeProfileId))]
    public TraineeProfile TraineeProfile { get; set; } = null!;

    [Required]
    public Guid ProductCategoryId { get; set; }

    [ForeignKey(nameof(ProductCategoryId))]
    public ProductCategory ProductCategory { get; set; } = null!;
}
