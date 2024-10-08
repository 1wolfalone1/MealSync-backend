using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Foods.Queries.GetAll;

public class GetAllShopFoodQuery : PaginationRequest, IQuery<Result>
{
    public long ShopId { get; set; }
}