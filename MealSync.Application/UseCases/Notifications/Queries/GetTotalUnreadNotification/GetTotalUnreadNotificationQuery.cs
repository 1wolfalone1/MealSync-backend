using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Notifications.Queries.GetTotalUnreadNotification;

public class GetTotalUnreadNotificationQuery : PaginationRequest, IQuery<Result>
{
}