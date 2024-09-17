using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Accounts.Commands;

public class CustomerLoginCommand : ICommand<Result>
{
    public AccountLoginRequest AccountLogin { get; set; }
}