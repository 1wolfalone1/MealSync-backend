using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Commands.VerifyAccountForUpdate;

public class VerifyAccountForUpdateCommand : ICommand<Result>
{
    public int Code { get; set; }
}