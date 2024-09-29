using MediatR;
using MealSync.Application.Shared;

namespace MealSync.Application.Common.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}