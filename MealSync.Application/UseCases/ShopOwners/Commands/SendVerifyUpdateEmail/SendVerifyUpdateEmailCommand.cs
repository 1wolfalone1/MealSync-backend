using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Commands.SendVerifyUpdateEmail;

public class SendVerifyUpdateEmailCommand : ICommand<Result>
{
    public bool IsVerifyOldEmail { get; set; }

    public string? NewEmail { get; set; } = null!;
}