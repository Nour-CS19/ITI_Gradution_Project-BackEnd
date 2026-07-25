using Femora.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Entities.Marketplace;
public class ProductCategory : BaseEntity
{
    [Required(ErrorMessage = "Category name is required")]
    [MaxLength(100, ErrorMessage = "Category name can't exceed 100 characters")]
    public string Name { get; set; }

    [MaxLength(500, ErrorMessage = "Description can't exceed 500 characters")]
    public string? Description { get; set; }

    // Navigation
    public ICollection<Product> Products { get; set; }
        = new List<Product>();

}
