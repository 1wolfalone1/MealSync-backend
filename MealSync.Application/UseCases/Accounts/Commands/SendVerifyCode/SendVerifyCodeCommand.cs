using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Accounts.Models;

namespace MealSync.Application.UseCases.Accounts.Commands.SendVerifyCode;

public class SendVerifyCodeCommand : ICommand<Result>
{
    public string Email { get; set; } = null!;

    public VerifyType VerifyType { get; set; }
}