using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.PlatformCategory.Commands.UpdatePlatformCategory;

public class UpdatePlatformCategoryCommand : ICommand<Result>
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsConfirm { get; set; }
}