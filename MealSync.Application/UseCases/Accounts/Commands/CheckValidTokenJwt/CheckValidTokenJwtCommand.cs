using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Accounts.Commands.CheckValidTokenJwt;

public class CheckValidTokenJwtCommand : ICommand<Result>
{
    public Domain.Enums.Roles? Role { get; set; }
}