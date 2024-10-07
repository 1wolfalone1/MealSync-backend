using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Favourites.Commands.MarkFavourite;

public class MarkFavouriteCommand : ICommand<Result>
{
    public long ShopId { get; set; }
}