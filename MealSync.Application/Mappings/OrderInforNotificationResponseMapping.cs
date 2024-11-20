using AutoMapper;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class OrderInforNotificationResponseMapping : Profile
{
    public OrderInforNotificationResponseMapping()
    {
        CreateMap<Account, OrderInforNotificationResponse.CustomerInforNotification>();
        CreateMap<Shop, OrderInforNotificationResponse.ShopInforNotification>()
            .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(
                src => src.LogoUrl))
            .ForMember(dest => dest.BannerUrl, opt => opt.MapFrom(
                src => src.BannerUrl))
            .ForMember(dest => dest.ShopName, opt => opt.MapFrom(
                src => src.Name))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(
                src => src.Account.FullName))
            .ForMember(dest => dest.ShopName, opt => opt.MapFrom(
                src => src.Account.Email))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(
                src => src.Account.AvatarUrl))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(
                src => src.Account.Email))
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(
                src => src.Account.RoleId));

        CreateMap<Account, OrderInforNotificationResponse.DeliveryStaffNotification>();

        CreateMap<Order, OrderInforNotificationResponse>()
            .ForMember(dest => dest.Customer, opt => opt.MapFrom(
                src => src.Customer.Account))
            .ForMember(dest => dest.Shop, opt => opt.MapFrom(
                src => src.Shop))
            .ForMember(dest => dest.DeliveryStaff, opt => opt.MapFrom(
                src => src.DeliveryPackage.ShopDeliveryStaff != null ? src.DeliveryPackage.ShopDeliveryStaff.Account : null));
    }
}