using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Deposits.Commands.ShopDeposit;

public class ShopDepositCommand : ICommand<Result>
{
    public double Amount { get; set; }
}