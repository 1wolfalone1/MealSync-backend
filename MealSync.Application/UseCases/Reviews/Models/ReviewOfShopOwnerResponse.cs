using MealSync.Application.Common.Models.Responses;

namespace MealSync.Application.UseCases.Reviews.Models;

public class ReviewOfShopOwnerResponse
{
    public ReviewOverviewDto ReviewOverview { get; set; }

    public PaginationResponse<ReviewOfShopOwnerDto> Reviews { get; set; }
}