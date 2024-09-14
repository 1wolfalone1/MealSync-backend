using MediatR;
using MealSync.Domain.Shared;

namespace MealSync.Application.Common.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
