using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Domain.Shared;

namespace MealSync.Application.UseCases.Accounts.Commands.LoginPassword;

public class LoginCommand : ICommand<Result>
{
    public string Email { get; set; }
    public string Password { get; set; }
}