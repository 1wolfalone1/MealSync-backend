using AutoMapper;
using MealSync.Application.Common.Services.Notifications.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class DeliveryPackageNotificationMapping : Profile
{
    public DeliveryPackageNotificationMapping()
    {
        CreateMap<DeliveryPackage, DeliveryPackageNotification>();
    }
}