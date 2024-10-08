using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.OptionGroups.Commands.CreateOptionGroup;

public class CreateOptionGroupCommand : ICommand<Result>
{
    public string Title { get; set; } = null!;

    public bool IsRequire { get; set; }

    public OptionGroupTypes Type { get; set; }

    public List<CreateOptionCommand> Options { get; set; }

    public class CreateOptionCommand
    {
        public bool IsDefault { get; set; }

        public string Title { get; set; } = null!;

        public bool IsCalculatePrice { get; set; }

        public double Price { get; set; }

        public string? ImageUrl { get; set; }
    }
}