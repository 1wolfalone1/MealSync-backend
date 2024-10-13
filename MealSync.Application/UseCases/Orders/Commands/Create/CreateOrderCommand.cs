using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.Create;

public class CreateOrderCommand : ICommand<Result>
{
    public long ShopId { get; set; }

    public string FullName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public long BuildingId { get; set; }

    public List<FoodOrderCommand> Foods { get; set; } = null!;

    public OrderTimeFrame OrderTime { get; set; }

    public long? VoucherId { get; set; }

    public double TotalDiscount { get; set; }

    public double TotalFoodCost { get; set; }

    public double TotalOrder { get; set; }

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

        public double Price { get; set; }

        public string? Note { get; set; }

        public List<OptionGroupRadioCommand>? OptionGroupRadio { get; set; }

        public List<OptionGroupCheckboxCommand>? OptionGroupCheckbox { get; set; }
    }

    public class OptionGroupRadioCommand
    {
        public long Id { get; set; }

        public OptionCommand Option { get; set; }
    }

    public class OptionGroupCheckboxCommand
    {
        public long Id { get; set; }

        public List<OptionCommand> Options { get; set; }
    }

    public class OptionCommand
    {
        public long Id { get; set; }

        public bool IsCalculatePrice { get; set; }

        public double Price { get; set; }
    }
}