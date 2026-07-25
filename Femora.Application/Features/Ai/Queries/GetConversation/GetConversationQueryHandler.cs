using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.Ai.Queries.GetConversation;

public class GetConversationQueryHandler(IAppDbContext db)
    : IRequestHandler<GetConversationQuery, GetConversationResponse>
{
    public async Task<GetConversationResponse> Handle(GetConversationQuery request, CancellationToken cancellationToken)
    {
        var conversation = await db.AIConversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId && c.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException("AIConversation", request.ConversationId.ToString());

        return new GetConversationResponse
        {
            ConversationId = conversation.Id,
            Title = conversation.Title,
            Messages = conversation.Messages
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageDto
                {
                    MessageId = m.Id,
                    Role = m.Role.ToString(),
                    Content = m.Content,
                    SentAt = m.SentAt
                })
                .ToList()
        };
    }
}
