using Femora.Domain.Common;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;
using Femora.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Entities.Marketplace;
public class Order : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Range(0.01, 999999)]
    public decimal TotalAmount { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; }
        = [];

    public Payment Payment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; }
}
