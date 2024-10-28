using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Customers.Commands.UpdateProfile;

public class UpdateProfileCommand : ICommand<Result>
{
    public string PhoneNumber { get; set; } = null!;

    public string? FullName { get; set; }

    public Genders Genders { get; set; }

    public long BuildingId { get; set; }
}