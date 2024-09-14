using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Domain.Shared;

namespace MealSync.Application.UseCases.Accounts.Commands;

public class CustomerLoginCommand : ICommand<Result>
{
    public AccountLoginRequest AccountLogin { get; set; }
}