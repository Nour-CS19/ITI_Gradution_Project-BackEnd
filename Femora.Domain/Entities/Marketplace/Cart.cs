using Femora.Domain.Common;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Femora.Domain.Entities.Marketplace;

public class Cart : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Timestamp]
    public byte[] RowVersion { get; set; }

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    public ApplicationUser User { get; set; }
}
