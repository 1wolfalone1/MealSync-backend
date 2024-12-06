using AutoMapper;
using MealSync.Application.Common.Enums;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Mappings;

public class DeliveryInforResponseMapping : Profile
{
    public DeliveryInforResponseMapping()
    {
        CreateMap<Order, DeliveryInforResponse>()
            .ForMember(dest => dest.DeliveryStatus, opt => opt.MapFrom(
                src => GetDeliveryStatus(src)))
            .ForMember(dest => dest.IsDeliveredByQR, opt => opt.MapFrom(
                src => string.IsNullOrEmpty(src.DeliverySuccessImageUrl)));
    }

    private int GetDeliveryStatus(Order order)
    {
        if (order.Status == OrderStatus.Delivered || order.ReceiveAt != default)
            return 1; // success

        if (order.Status == OrderStatus.FailDelivery || order.LastestDeliveryFailAt != default)
            return 2; // fail

        return 0;
    }
}