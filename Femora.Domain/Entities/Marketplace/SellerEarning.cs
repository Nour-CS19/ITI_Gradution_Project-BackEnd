using Femora.Domain.Common;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Entities.Marketplace;
public class SellerEarning : BaseEntity
{
    public Guid SellerProfileId { get; set; }
    public Guid OrderItemId { get; set; }
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }
    public EarningStatus Status { get; set; } = EarningStatus.Pending;
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }

    // Navigation
    public SellerProfile SellerProfile { get; set; } = null!;
    public OrderItem OrderItem { get; set; } = null!;

    // Mirrors InstructorEarning's platform fee model — adjust if marketplace
    // commission should differ from the LMS course commission.
    public static decimal CalculatePlatformFee(decimal amount) => amount * 0.30m;
}
