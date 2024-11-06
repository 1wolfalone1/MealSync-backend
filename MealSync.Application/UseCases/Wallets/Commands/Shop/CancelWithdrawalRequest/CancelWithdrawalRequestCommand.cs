using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Wallets.Commands.Shop.CancelWithdrawalRequest;

public class CancelWithdrawalRequestCommand : ICommand<Result>
{
    public long Id { get; set; }

    public bool IsConfirm { get; set; }
}