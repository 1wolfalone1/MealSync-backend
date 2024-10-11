using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Options.Models;

public class OptionResponse
{
    public long Id { get; set; }

    public long OptionGroupId { get; set; }

    public bool IsDefault { get; set; }

    public string Title { get; set; } = null!;

    public bool IsCalculatePrice { get; set; }

    public double Price { get; set; }

    public string? ImageUrl { get; set; }

    public OptionStatus Status { get; set; }
}