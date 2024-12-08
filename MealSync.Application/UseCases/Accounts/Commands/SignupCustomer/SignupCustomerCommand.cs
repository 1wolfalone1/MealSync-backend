using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Accounts.Commands.SignupCustomer;

public class SignupCustomerCommand : ICommand<Result>
{
    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public Genders Gender { get; set; } = Genders.UnKnown;

    public string FullName { get; set; }
}