using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Foods.Queries.TopFood;

public class GetTopFoodQuery : PaginationRequest, IQuery<Result>
{

}