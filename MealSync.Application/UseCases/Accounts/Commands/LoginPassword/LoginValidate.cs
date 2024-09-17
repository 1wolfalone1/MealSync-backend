using FluentValidation;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;

namespace MealSync.Application.UseCases.Accounts.Commands.LoginPassword;

public class LoginValidate : AbstractValidator<LoginCommand>
{
    public LoginValidate(ISystemResourceRepository systemResourceRepository)
    {
        RuleFor(e => e.Email)
            .NotEmpty()
            .WithMessage(systemResourceRepository.GetByResourceCode(MessageCode.VL00001.ToString()));
        RuleFor(e => e.Password)
            .NotEmpty()
            .WithMessage(systemResourceRepository.GetByResourceCode(MessageCode.VL00002.ToString()));
    }
}