using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.ProfileApplications.Queries.GetMy;

public class GetMyProfileApplicationQuery : IRequest<MyProfileApplicationDto?>
{
    public Guid UserId { get; set; }
}

public class MyProfileApplicationDto
{
    public Guid Id { get; set; }
    public string RequestedRole { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? Bio { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? StoreName { get; set; }
}

public class GetMyProfileApplicationQueryHandler : IRequestHandler<GetMyProfileApplicationQuery, MyProfileApplicationDto?>
{
    private readonly IAppDbContext _context;

    public GetMyProfileApplicationQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<MyProfileApplicationDto?> Handle(GetMyProfileApplicationQuery request, CancellationToken cancellationToken)
    {
        var app = await _context.ProfileApplicationRequests
            .Where(r => r.UserId == request.UserId)
            .OrderByDescending(r => r.SubmittedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (app == null)
            return null;

        return new MyProfileApplicationDto
        {
            Id = app.Id,
            RequestedRole = app.RequestedRole.ToString(),
            Status = app.Status.ToString(),
            SubmittedAt = app.SubmittedAt,
            ReviewedAt = app.ReviewedAt,
            RejectionReason = app.RejectionReason,
            Bio = app.Bio,
            PortfolioUrl = app.PortfolioUrl,
            StoreName = app.StoreName
        };
    }
}
