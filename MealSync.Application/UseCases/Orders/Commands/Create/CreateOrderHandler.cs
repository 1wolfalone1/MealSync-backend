using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.Create;

public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, Result>
{

    public Task<Result<Result>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Check shop is active and currently accepting orders.

        //
        throw new NotImplementedException();
    }
}