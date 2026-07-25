using Femora.Domain.Common;
using Femora.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Entities.Admin;
public class ApprovalRequest : BaseEntity
{
    public Guid? AdminId { get; set; }
    public ApplicationUser? ReviwedBy{ get; set; }
    public Guid RequsterId { get; set; }
    public ApplicationUser? RequestedBy{ get; set; }
    public Guid EntityId { get; set; }
    public ApprovalEntityType Type { get; set; }
    public string Note { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    public DateTime? ReviewedAt { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;




}
