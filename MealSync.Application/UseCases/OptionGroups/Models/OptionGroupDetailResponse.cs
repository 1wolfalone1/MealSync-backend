using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.OptionGroups.Models;

public class OptionGroupDetailResponse
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public bool IsRequire { get; set; }

    public OptionGroupTypes Type { get; set; }

    public OptionGroupStatus Status { get; set; }

    public int MinChoices { get; set; }

    public int MaxChoices { get; set; }

    public List<OptionResponse> Options { get; set; }

    public List<FoodInOptionGroupResponse> Foods { get; set; }

    public class OptionResponse
    {
        public long Id { get; set; }

        public bool IsDefault { get; set; }

        public string Title { get; set; } = null!;

        public bool IsCalculatePrice { get; set; }

        public double Price { get; set; }

        public string? ImageUrl { get; set; }

        public OptionStatus Status { get; set; }
    }

    public class FoodInOptionGroupResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public double Price { get; set; }

        public string? Description { get; set; }

        public string ImageUrl { get; set; } = null!;

        public bool IsSoldOut { get; set; }

        public FoodStatus Status { get; set; }
    }
}