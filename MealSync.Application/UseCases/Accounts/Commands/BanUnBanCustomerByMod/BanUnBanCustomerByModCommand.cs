using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Accounts.Commands.BanUnBanCustomerByMod;

public class BanUnBanCustomerByModCommand : ICommand<Result>
{
    public long CustomerId { get; set; }

    public AccountStatus Status { get; set; }

    public bool IsConfirm { get; set; }

    public string? Reason { get; set; }
}