using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Admin.Commands.UpdateOrderStatus
{
    public sealed class UpdateOrderStatusCommandHandler(IAppDbContext db)
        : IRequestHandler<UpdateOrderStatusCommand>
    {
        public async Task Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var updated = await db.Orders
                .Where(o => o.Id == request.OrderId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(o => o.Status, request.Status),
                    cancellationToken);

            if (updated == 0)
            {
                throw new NotFoundException("Order", request.OrderId.ToString());
            }
        }
    }
}
