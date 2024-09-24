using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Accounts.Commands.ShopRegister;

public class ShopRegisterCommand : ICommand<Result>
{
    public string Email { get; set; }

    public string FullName { get; set; }

    public string PhoneNumber { get; set; }

    public Genders Gender { get; set; } = Genders.Unknow;

    public string Password { get; set; }

    public string ShopName { get; set; }

    public long[] DormitoryIds { get; set; }

    public string Address { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}