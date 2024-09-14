using MediatR;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Domain.Entities;
using MealSync.Domain.Shared;

namespace MealSync.Application.UseCases.Accounts.Queries;

public class GetAllAccountQuery :  IQuery<Result<List<Account>>>
{
}
