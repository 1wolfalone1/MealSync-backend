using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Products.Commands.Create;

public class CreateProductCommand : ICommand<Result>
{
    public string Name { get; set; } = null!;

    public string ImgUrl { get; set; } = null!;

    public string? Description { get; set; } = null!;

    public float Price { get; set; }

    public List<long> CategoryIds { get; set; } = null!;

    public List<OperatingHourCommand> OperatingHours { get; set; }

    public List<CreateQuestionCommand>? Questions { get; set; }

    public class OperatingHourCommand
    {
        public long OperatingDayId { get; set; }

        public int StartTime { get; set; }

        public int EndTime { get; set; }
    }

    public class CreateQuestionCommand
    {
        public OptionGroupTypes Type { get; set; }

        public string Description { get; set; } = null!;

        public List<CreateOptionCommand> Options { get; set; }
    }

    public class CreateOptionCommand
    {
        public string Description { get; set; } = null!;

        public bool IsPricing { get; set; }

        public string? ImageUrl { get; set; }

        public float Price { get; set; }
    }
}