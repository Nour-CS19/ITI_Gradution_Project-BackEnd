using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Models;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.ProfileApplications.Queries.GetList;

public class GetProfileApplicationsQuery : IRequest<PagedResult<ProfileApplicationDto>>
{
    public ApplicationRequestStatus? Status { get; set; }
    public RequestedRole? RequestedRole { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ProfileApplicationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string RequestedRole { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    
    public string? Bio { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? NationalIdNumber { get; set; }
    public string? StoreName { get; set; }
    public string? StoreDescription { get; set; }
}

public class GetProfileApplicationsQueryHandler : IRequestHandler<GetProfileApplicationsQuery, PagedResult<ProfileApplicationDto>>
{
    private readonly IAppDbContext _context;

    public GetProfileApplicationsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProfileApplicationDto>> Handle(GetProfileApplicationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ProfileApplicationRequests
            .Include(r => r.User)
            .AsNoTracking();

        // Apply filters
        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        if (request.RequestedRole.HasValue)
        {
            query = query.Where(r => r.RequestedRole == request.RequestedRole.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var pageNumber = Math.Max(request.PageNumber, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(r => r.SubmittedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ProfileApplicationDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserFullName = r.User.FirstName + " " + r.User.LastName,
                UserEmail = r.User.Email ?? string.Empty,
                RequestedRole = r.RequestedRole.ToString(),
                Status = r.Status.ToString(),
                SubmittedAt = r.SubmittedAt,
                Bio = r.Bio,
                PortfolioUrl = r.PortfolioUrl,
                NationalIdNumber = r.NationalIdNumber,
                StoreName = r.StoreName,
                StoreDescription = r.StoreDescription
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProfileApplicationDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
