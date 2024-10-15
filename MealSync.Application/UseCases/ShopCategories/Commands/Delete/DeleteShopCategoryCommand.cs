using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopCategories.Commands.Delete;

public class DeleteShopCategoryCommand : ICommand<Result>
{
    public long Id { get; set; }
}