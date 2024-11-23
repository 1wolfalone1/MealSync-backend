using FluentValidation;

namespace MealSync.Application.UseCases.WithdrawalRequests.Commands.UpdateWithdrawalRequestStatus;

public class UpdateWithdrawalRequestStatusValidate : AbstractValidator<UpdateWithdrawalRequestStatusCommand>
{
    public UpdateWithdrawalRequestStatusValidate()
    {

    }
}