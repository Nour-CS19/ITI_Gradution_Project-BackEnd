using Femora.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Entities.Marketplace;
public class ProductVariant : BaseEntity
{
    [Required]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Variant name is required")]
    [MaxLength(100, ErrorMessage = "Variant name can't exceed 100 characters")]
    public string Name { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity can't be negative")]
    public int StockQuantity { get; set; }

    [MaxLength(50, ErrorMessage = "Color can't exceed 50 characters")]
    public string? Color { get; set; }

    [MaxLength(50, ErrorMessage = "Size can't exceed 50 characters")]
    public string? Size { get; set; }

    [MaxLength(100, ErrorMessage = "Material can't exceed 100 characters")]
    public string? Material { get; set; }

    // Navigation
    public Product? Product { get; set; }

    public ICollection<CartItem> CartItems { get; set; }
        = new List<CartItem>();

    public ICollection<OrderItem> OrderItems { get; set; }
        = new List<OrderItem>();



}
