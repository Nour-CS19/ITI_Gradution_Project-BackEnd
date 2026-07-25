using Femora.Application.Features.LMS.Dashboard.DTOs;
using MediatR;

namespace Femora.Application.Features.LMS.Dashboard.Queries.GetTraineeDashboardStats;

/// <summary>Stats + progress + upcoming quiz + achievements for the current trainee's dashboard.</summary>
public record GetTraineeDashboardStatsQuery : IRequest<TraineeDashboardStatsDto>;
