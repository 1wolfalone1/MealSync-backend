using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.PlatformCategory.Commands.CreatePlatformCategory;

public class CreatePlatformCategoryCommand : ICommand<Result>
{
    public string Name { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; }
}