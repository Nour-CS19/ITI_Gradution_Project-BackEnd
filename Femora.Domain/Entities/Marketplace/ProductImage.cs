using Femora.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Entities.Marketplace;
public class ProductImage : BaseEntity
{
    [Required]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Image URL is required")]
    [MaxLength(500, ErrorMessage = "Image URL can't exceed 500 characters")]
    [Url(ErrorMessage = "Invalid URL format")]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Order index can't be negative")]
    public int OrderIndex { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
}
