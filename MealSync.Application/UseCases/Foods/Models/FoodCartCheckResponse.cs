using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Models;

public class FoodCartCheckResponse
{
    public List<string> IdsRequest { get; set; }

    public bool IsRemoveAllCart { get; set; }

    public bool IsReceivingOrderPaused { get; set; }

    public string MessageForAllCart { get; set; }

    public bool IsPresentFoodNeedRemoveToday { get; set; }

    public string? MessageFoodRemoveToday { get; set; }

    public List<string>? IdsNotFoundToday { get; set; }

    public bool IsPresentFoodNeedRemoveTomorrow { get; set; }

    public string? MessageFoodRemoveTomorrow { get; set; }

    public List<string>? IdsNotFoundTomorrow { get; set; }

    public List<DetailFoodResponse> Foods { get; set; }

    public class DetailFoodResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public double Price { get; set; }

        public string? ImageUrl { get; set; }

        public long ShopId { get; set; }

        public List<OptionGroupRadioResponse> OptionGroupRadio { get; set; }

        public List<OptionGroupCheckboxResponse> OptionGroupCheckbox { get; set; }
    }

    public class OptionGroupRadioResponse
    {
        public long Id { get; set; }

        public string? Title { get; set; }

        public OptionResponse Option { get; set; }
    }

    public class OptionGroupCheckboxResponse
    {
        public long Id { get; set; }

        public string? Title { get; set; }

        public List<OptionResponse> Options { get; set; }
    }

    public class OptionResponse
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsCalculatePrice { get; set; }

        public double Price { get; set; }
    }
}