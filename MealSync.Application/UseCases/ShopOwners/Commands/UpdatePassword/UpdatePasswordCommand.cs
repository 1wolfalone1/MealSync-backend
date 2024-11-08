using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdatePassword;

public class UpdatePasswordCommand : ICommand<Result>
{
    public string OldPassword { get; set; } = null!;

    public string NewPassword { get; set; } = null!;
}