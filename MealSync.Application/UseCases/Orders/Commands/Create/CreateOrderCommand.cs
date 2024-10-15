using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Orders.Commands.Create;

public class CreateOrderCommand : ICommand<Result>
{
    public long ShopId { get; set; }

    public string FullName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public long BuildingId { get; set; }

    public List<FoodOrderCommand> Foods { get; set; } = null!;

    public string? Note { get; set; }

    public OrderTimeFrame OrderTime { get; set; }

    public long? VoucherId { get; set; }

    public double TotalDiscount { get; set; }

    public double TotalFoodCost { get; set; }

    public double TotalOrder { get; set; }

    public PaymentMethods PaymentMethod { get; set; }

    public ShipInfoCommand ShipInfo { get; set; }

    public class OrderTimeFrame
    {
        public bool IsOrderNextDay { get; set; }

        public int StartTime { get; set; }

        public int EndTime { get; set; }
    }

    public class FoodOrderCommand
    {
        public string Id { get; set; } = null!;

        public int Quantity { get; set; }

        public List<OptionGroupRadioCommand>? OptionGroupRadio { get; set; }

        public List<OptionGroupCheckboxCommand>? OptionGroupCheckbox { get; set; }
    }

    public class OptionGroupRadioCommand
    {
        public long Id { get; set; }

        public long OptionId { get; set; }
    }

    public class OptionGroupCheckboxCommand
    {
        public long Id { get; set; }

        public List<long> OptionIds { get; set; }
    }

    public class ShipInfoCommand
    {
        public double Distance { get; set; }

        public double Duration { get; set; }
    }
}