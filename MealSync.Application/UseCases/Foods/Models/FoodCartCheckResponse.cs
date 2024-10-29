using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Models;

public class FoodCartCheckResponse
{
    public List<DetailFoodResponse> Foods { get; set; }

    public List<string> IdNotFounds { get; set; }

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