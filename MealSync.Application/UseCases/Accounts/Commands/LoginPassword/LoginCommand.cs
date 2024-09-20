using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Accounts.Commands.LoginPassword;

public class LoginCommand : ICommand<Result>
{
    public LoginContextType LoginContext { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }
}