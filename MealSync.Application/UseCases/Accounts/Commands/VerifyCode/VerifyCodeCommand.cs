using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Accounts.Models;

namespace MealSync.Application.UseCases.Accounts.Commands.VerifyCode;

public class VerifyCodeCommand : ICommand<Result>
{
    public string Email { get; set; } = null!;

    public int Code { get; set; }

    public VerifyType VerifyType { get; set; }

    public string? Password { get; set; }
}