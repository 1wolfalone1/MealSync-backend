using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Notifications.Queries.GetModeratorNotification;

public class GetModeratorNotificationQuery : PaginationRequest, IQuery<Result>
{
}