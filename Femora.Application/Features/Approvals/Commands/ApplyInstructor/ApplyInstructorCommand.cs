using MediatR;

namespace Femora.Application.Features.Approvals.Commands.ApplyInstructor;

public class ApplyInstructorCommand : IRequest<Guid>
{
    public Guid UserId { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string? PortfolioUrl { get; set; }
}