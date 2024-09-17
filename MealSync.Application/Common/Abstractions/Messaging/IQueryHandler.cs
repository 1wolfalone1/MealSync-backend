using MediatR;
using MealSync.Application.Shared;

namespace MealSync.Application.Common.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}