using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Wallets.Commands.Shop.VerifyWithdrawalRequest;

public class WithdrawalRequestCommand : ICommand<Result>
{
    public long Amount { get; set; }

    public string BankCode { get; set; }

    public string BankShortName { get; set; }

    public string BankAccountNumber { get; set; }

    public int VerifyCode { get; set; }
}