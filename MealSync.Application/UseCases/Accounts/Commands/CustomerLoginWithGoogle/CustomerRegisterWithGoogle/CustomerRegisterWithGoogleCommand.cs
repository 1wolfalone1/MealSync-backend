using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Accounts.Commands.CustomerLoginWithGoogle.CustomerRegisterWithGoogle;

public class CustomerRegisterWithGoogleCommand : ICommand<Result>
{
    public int CodeConfirm { get; set; }

    public string PhoneNumber { get; set; }

    public long BuildingId { get; set; }

    public string Email { get; set; }

    public string FUserId { get; set; }

    public string DeviceToken { get; set; }

    public string AvatarUrl { get; set; }

    public string Name { get; set; }
}