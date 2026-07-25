using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Products.DTOs
{
    public class ProductDetailsDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public bool IsPublished { get; set; }

        /// <summary>Draft | PendingApproval | Approved | Rejected</summary>
        public string Status { get; set; } = "Draft";

        public string? AdminNote { get; set; }

        public List<string> Images { get; set; } = [];

        public List<ProductVariantDto> Variants { get; set; } = [];

        public string? SellerName { get; set; }
        public string? SellerStoreName { get; set; }
    }
}
