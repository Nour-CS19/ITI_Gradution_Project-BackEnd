using Femora.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Entities.Marketplace;
public class OrderItem :BaseEntity
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public Guid ProductVariantId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }

    [Range(0.01, 999999)]
    public decimal UnitPrice { get; set; }

    public Order Order { get; set; } = null;

    public ProductVariant ProductVariant { get; set; } = null;
}
