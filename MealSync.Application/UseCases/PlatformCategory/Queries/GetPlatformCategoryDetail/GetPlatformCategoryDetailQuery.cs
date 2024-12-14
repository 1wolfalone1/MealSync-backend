using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.PlatformCategory.Queries.GetPlatformCategoryDetail;

public class GetPlatformCategoryDetailQuery : IQuery<Result>
{
    public long Id { get; set; }
}