using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateEmail;

public class UpdateEmailCommand : ICommand<Result>
{
    public int CodeVerifyOldEmail { get; set; }

    public int CodeVerifyNewEmail { get; set; }

    public string NewEmail { get; set; } = null!;
}