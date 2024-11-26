using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Notifications.Queries.GetTotalUnreadCustomerNotification;

public class GetTotalUnreadCustomerNotificationQuery : PaginationRequest, IQuery<Result>
{
}