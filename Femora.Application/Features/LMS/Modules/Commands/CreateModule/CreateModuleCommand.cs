using MediatR;

namespace Femora.Application.Features.LMS.Modules.Commands.CreateModule;

public class CreateModuleCommand : IRequest<Guid>
{
    public Guid CourseId { get; set; }
    public string Title { get; set; }
    public int OrderIndex { get; set; }
}