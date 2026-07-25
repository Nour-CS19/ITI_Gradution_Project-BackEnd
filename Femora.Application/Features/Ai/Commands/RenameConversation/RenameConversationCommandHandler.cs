using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.Ai.Commands.RenameConversation;

public class RenameConversationCommandHandler(IAppDbContext db)
    : IRequestHandler<RenameConversationCommand, Unit>
{
    public async Task<Unit> Handle(RenameConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await db.AIConversations
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId && c.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException("AIConversation", request.ConversationId.ToString());

        conversation.Title = request.Title.Trim();
        conversation.UpdatedAt = System.DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
