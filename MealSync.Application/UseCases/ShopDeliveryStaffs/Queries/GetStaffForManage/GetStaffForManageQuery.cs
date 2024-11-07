using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Queries.GetStaffForManage;

public class GetStaffForManageQuery : PaginationRequest, IQuery<Result>
{
    public string? SearchValue { get; set; }

    public ShopDeliveryStaffStatus? Status { get; set; }
}