namespace Femora.Domain.Enums;

/// <summary>
/// Derived, seller-facing lifecycle status of a Product.
/// Not persisted as a column — computed from Product.IsPuplished + the latest
/// ApprovalRequest (Type = ProductApproval) so we don't duplicate state.
/// </summary>
public enum ProductStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3
}
