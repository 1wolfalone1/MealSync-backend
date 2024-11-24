using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.WithdrawalRequests.Commands.UpdateWithdrawalRequestStatus;

public class UpdateWithdrawalRequestStatusCommand : ICommand<Result>
{
    public long Id { get; set; }

    public WithdrawalRequestStatus Status { get; set; }

    public string? Reason { get; set; }

    public bool IsConfirm { get; set; }
}