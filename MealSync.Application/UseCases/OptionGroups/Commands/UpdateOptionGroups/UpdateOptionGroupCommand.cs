using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.OptionGroups.Commands.CreateOptionGroup;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.OptionGroups.Commands.UpdateOptionGroups;

public class UpdateOptionGroupCommand : ICommand<Result>
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public bool IsRequire { get; set; }

    public OptionGroupTypes Type { get; set; }

    public int MinChoices { get; set; }

    public int MaxChoices { get; set; }

    public OptionGroupStatus Status { get; set; }

    public List<UpdateOptionRequest> Options { get; set; }
}

public class UpdateOptionRequest
{
    public long Id { get; set; }

    public bool IsDefault { get; set; }

    public string Title { get; set; } = null!;

    public OptionStatus Status { get; set; }

    public bool IsCalculatePrice { get; set; }

    public double Price { get; set; }

    public string? ImageUrl { get; set; }
}