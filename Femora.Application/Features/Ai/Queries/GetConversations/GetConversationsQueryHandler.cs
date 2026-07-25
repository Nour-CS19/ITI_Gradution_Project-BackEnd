using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.Ai.Queries.GetConversations;

public class GetConversationsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetConversationsQuery, List<ConversationSummaryDto>>
{
    public async Task<List<ConversationSummaryDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        return await db.AIConversations
            .Where(c => c.UserId == request.UserId)
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => new ConversationSummaryDto
            {
                ConversationId = c.Id,
                Title = c.Title,
                Context = c.Context.ToString(),
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
