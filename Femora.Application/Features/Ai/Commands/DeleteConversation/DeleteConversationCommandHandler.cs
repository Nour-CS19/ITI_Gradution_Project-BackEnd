using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.Ai.Commands.DeleteConversation;

public class DeleteConversationCommandHandler(IAppDbContext db)
    : IRequestHandler<DeleteConversationCommand, Unit>
{
    public async Task<Unit> Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await db.AIConversations
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId && c.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException("AIConversation", request.ConversationId.ToString());

        var messages = db.AIMessages.Where(m => m.ConversationId == conversation.Id);
        db.AIMessages.RemoveRange(messages);
        db.AIConversations.Remove(conversation);

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
