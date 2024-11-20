using AutoMapper;
using MealSync.Application.UseCases.Notifications.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class NotificationResponseMapping : Profile
{
    public NotificationResponseMapping()
    {
        CreateMap<Notification, NotificationResponse.NotificationInfor>();
    }
}