using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopCategories.Commands.Rearrange;

public class RearrangeShopCategoryCommand : ICommand<Result>
{
    public List<long> Ids { get; set; }
}