using Femora.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Entities.Marketplace;
public class CartItem :BaseEntity
{
    [Required]
    public Guid CartId { get; set; }

    [Required]
    public Guid ProductVariantId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }

    public Cart Cart { get; set; }

    public ProductVariant ProductVariant { get; set; }
}
