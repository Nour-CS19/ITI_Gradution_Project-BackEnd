using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Approvals.Common;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Entities.Marketplace;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace Femora.Application.Features.Approvals.Commands.ReviewApproval;

public class ReviewApprovalCommandHandler : IRequestHandler<ReviewApprovalCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewApprovalCommandHandler(IAppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<bool> Handle(ReviewApprovalCommand request, CancellationToken cancellationToken)
    {
        var approval = await _context.ApprovalRequests
            .FirstOrDefaultAsync(x => x.Id == request.ApprovalId && x.ApprovalStatus == ApprovalStatus.Pending, cancellationToken);

        if (approval is null)
            throw new NotFoundException(approval.ToString(), approval.Id.ToString());

        approval.AdminId = request.AdminId;
        approval.ReviewedAt = DateTime.UtcNow;
        approval.ApprovalStatus = request.IsApproved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;

        var payload = ApprovalNotePayload.Parse(approval.Note);
        if (request.Note is not null)
            payload.AdminNote = request.Note;
        approval.Note = payload.ToJson();

        if (request.IsApproved)
        {
            switch (approval.Type)
            {
                case ApprovalEntityType.InstructorVerification:
                case ApprovalEntityType.SellerVerification:
                {
                    var user = await _userManager.FindByIdAsync(approval.RequsterId.ToString());
                    if (user is null)
                        throw new NotFoundException(nameof(ApplicationUser), approval.RequsterId.ToString());

                    if (approval.Type == ApprovalEntityType.InstructorVerification)
                    {
                        // Portfolio link has no dedicated column yet, so it's appended to Bio.
                        var instructorBio = payload.Bio ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(payload.Portfolio))
                            instructorBio = string.IsNullOrWhiteSpace(instructorBio)
                                ? $"Portfolio: {payload.Portfolio}"
                                : $"{instructorBio}\nPortfolio: {payload.Portfolio}";

                        if (user.InstructorProfile is null)
                        {
                            var instructor = new InstructorProfile
                            {
                                UserId = user.Id,
                                Specialization = string.Empty,
                                Bio = instructorBio,
                                Rating = 0f,
                                TotalEarnings = 0m,
                                Status = VerificationStatus.Approved,
                                VerifiedByAdminId = request.AdminId,
                                VerifiedAt = DateTime.UtcNow
                            };
                            _context.InstructorProfiles.Add(instructor);
                            user.InstructorProfile = instructor;
                        }
                        else
                        {
                            user.InstructorProfile.Bio = instructorBio;
                            user.InstructorProfile.Status = VerificationStatus.Approved;
                            user.InstructorProfile.VerifiedByAdminId = request.AdminId;
                            user.InstructorProfile.VerifiedAt = DateTime.UtcNow;
                            _context.InstructorProfiles.Update(user.InstructorProfile);
                        }
                    }
                    else
                    {
                        if (user.SellerProfile is null)
                        {
                            var seller = new SellerProfile
                            {
                                UserId = user.Id,
                                StoreName = payload.ShopName ?? string.Empty,
                                StoreDescription = payload.Description ?? string.Empty,
                                Rating = 0f,
                                TotalEarnings = 0m,
                                TaxAmount = 0m,
                                Status = VerificationStatus.Approved,
                                VerifiedByAdminId = request.AdminId,
                                VerifiedAt = DateTime.UtcNow
                            };
                            _context.SellerProfiles.Add(seller);
                            user.SellerProfile = seller;
                        }
                        else
                        {
                            user.SellerProfile.StoreName = payload.ShopName ?? user.SellerProfile.StoreName;
                            user.SellerProfile.StoreDescription = payload.Description ?? user.SellerProfile.StoreDescription;
                            user.SellerProfile.Status = VerificationStatus.Approved;
                            user.SellerProfile.VerifiedByAdminId = request.AdminId;
                            user.SellerProfile.VerifiedAt = DateTime.UtcNow;
                            _context.SellerProfiles.Update(user.SellerProfile);
                        }
                    }
                    break;
                }

                case ApprovalEntityType.CourseApproval:
                {
                    var course = await _context.Courses
                        .FirstOrDefaultAsync(x => x.Id == approval.EntityId, cancellationToken);
                    if (course is null)
                        throw new NotFoundException(nameof(Course), approval.EntityId.ToString());

                    course.IsPublished = true;
                    course.RequiresApproval = false;
                    course.Status = CourseStatus.Published;
                    break;
                }

                case ApprovalEntityType.ProductApproval:
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(x => x.Id == approval.EntityId, cancellationToken);
                    if (product is null)
                        throw new NotFoundException(nameof(Product), approval.EntityId.ToString());

                    product.IsPuplished = true;
                    break;
                }

                default:
                    throw new InvalidOperationException("Unknown approval type.");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}