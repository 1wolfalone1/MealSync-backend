using AutoMapper;
using MealSync.Application.UseCases.Notifications.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class NotificationCustomerResponseMapping : Profile
{
    public NotificationCustomerResponseMapping()
    {
        CreateMap<Notification, NotificationCustomerResponse.NotificationInfor>()
            .ForMember(dest => dest.CreatedDateTime, opt => opt.MapFrom(
                src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDateTime, opt => opt.MapFrom(
                src => src.UpdatedDate));
    }
}