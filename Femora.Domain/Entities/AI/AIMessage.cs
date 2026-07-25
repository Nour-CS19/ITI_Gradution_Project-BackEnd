using Femora.Domain.Common;
using Femora.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Entities.AI;

public class AIMessage : BaseEntity
{
    [Required]
    public Guid ConversationId { get; set; }

    [ForeignKey(nameof(ConversationId))]
    public AIConversation Conversation { get; set; } = null!;

    [Required]
    public AIMessageRole Role { get; set; }

    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    public string? ResourceDocumentationJson { get; set; }

    [Required]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}