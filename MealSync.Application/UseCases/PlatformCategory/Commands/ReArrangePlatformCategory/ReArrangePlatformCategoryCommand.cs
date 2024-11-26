using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.PlatformCategory.Commands.ReArrangePlatformCategory;

public class ReArrangePlatformCategoryCommand : ICommand<Result>
{
    public long[] Ids { get; set; }
}