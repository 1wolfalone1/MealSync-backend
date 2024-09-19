using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Accounts.Commands.LoginPassword;

public class LoginCommand : ICommand<Result>
{
    public int Role { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}