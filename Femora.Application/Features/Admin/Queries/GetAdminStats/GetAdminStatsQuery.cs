using MediatR;

namespace Femora.Application.Features.Admin.Queries.GetAdminStats;

public sealed record GetAdminStatsQuery : IRequest<AdminStatsDto>;