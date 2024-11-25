using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Accounts.Commands.UpdateDeviceToken;

public class UpdateDeviceTokenCommand : ICommand<Result>
{
    public string DeviceToken { get; set; }
}