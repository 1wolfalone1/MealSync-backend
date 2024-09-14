using MediatR;
using MealSync.Domain.Shared;

namespace MealSync.Application.Common.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
