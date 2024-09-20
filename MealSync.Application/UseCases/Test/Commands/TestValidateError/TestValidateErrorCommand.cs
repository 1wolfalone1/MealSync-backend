using FluentValidation;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Test.Commands.TestValidateError;

public class TestValidateErrorCommand : ICommand<Result>
{
    public long Id { get; set; }

    public string Email { get; set; }
}

public class TestValidator : AbstractValidator<TestValidateErrorCommand>
{
    public TestValidator()
    {
        RuleFor(x => x.Id)
            .LessThan(0)
            .WithMessage("số không thể nho hon 1");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email khong thể trống");
    }
}