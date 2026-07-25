using Femora.Domain.Common;
using Femora.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Entities.Marketplace;
public class Product : BaseEntity
{
    [Required]
    public Guid SellerProfileId { get; set; }

    [Required]
    public Guid ProductCategoryId { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [MaxLength(100, ErrorMessage = "Name can't exceed 100 characters")]
    public string Name { get; set; }

    [MaxLength(1000, ErrorMessage = "Description can't exceed 1000 characters")]
    public string? Description { get; set; }

    public bool IsPuplished { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Navigation
    public SellerProfile? SellerProfile { get; set; }

    public ProductCategory? ProductCategory { get; set; }

    public ICollection<ProductVariant> ProductVariants { get; set; }
        = new List<ProductVariant>();

    public ICollection<ProductImage> ProductImages { get; set; }
        = new List<ProductImage>();

}
