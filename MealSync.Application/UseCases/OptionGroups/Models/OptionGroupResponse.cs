using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.OptionGroups.Models;

public class OptionGroupResponse
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public bool IsRequire { get; set; }

    public OptionGroupTypes Type { get; set; }

    public OptionGroupStatus Status { get; set; }

    public List<OptionResponse> Options { get; set; }

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
}