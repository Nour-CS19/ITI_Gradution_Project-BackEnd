using Femora.Domain.Common;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;
using Femora.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Entities.AI;

public class AIRecommendation : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

 

    [Required]
    public AIRecommendationType Type { get; set; }

    [Required]
    public Guid EntityId { get; set; }

    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    public bool IsViewed { get; set; } = false;

    [Required]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public string? ReasonJson { get; set; }

    [Required]
    [Range(0.0, 1.0)]
    public double Score { get; set; }

    public ApplicationUser User { get; set; }
}