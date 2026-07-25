using Femora.Domain.Common;
using Femora.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Entities.Marketplace;
public class Payment : BaseEntity
{
    public Guid UserId { get; set; }
    // OrderId is nullable because some payments (e.g. course enrollments) reference an enrollment
    public Guid? OrderId { get; set; }

    // Optional reference to an enrollment when the payment is for a course purchase
    public Guid? EnrollmentId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    [Required]
    [MaxLength(50)]
    public string? PaymentStatus { get; set; }

    [MaxLength(200)]
    public string? TransactionReference { get; set; }

    public DateTime PaidAt { get; set; } = DateTime.UtcNow;

    public Order? Order { get; set; }
    public Domain.Entities.LMS.Enrollment? Enrollment { get; set; }
    public ApplicationUser User { get; set; }
}
