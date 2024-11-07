using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Queries.GetDetailById;

public class GetDetailByIdQuery : IQuery<Result>
{
    public long Id { get; set; }
}