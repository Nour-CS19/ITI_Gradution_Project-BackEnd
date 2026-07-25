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

public class AIConversation : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [Required]
    public AIConversationContext Context { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AIMessage> Messages { get; set; } = new List<AIMessage>();
}