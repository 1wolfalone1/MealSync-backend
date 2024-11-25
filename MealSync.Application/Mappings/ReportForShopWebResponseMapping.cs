using AutoMapper;
using MealSync.Application.UseCases.Reports.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class ReportForShopWebResponseMapping : Profile
{
    public ReportForShopWebResponseMapping()
    {
        CreateMap<DeliveryPackage, ReportForShopWebResponse.ShopDeliveryStaffInforInReport>()
            .ForMember(dest => dest.DeliveryPackageId, opt => opt.MapFrom(
                src => src.Id))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(
                src => GetDeliveryPackageId(src)))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(
                src => src.ShopDeliveryStaffId.HasValue ? src.ShopDeliveryStaff.Account.FullName : src.Shop.Account.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(
                src => src.ShopDeliveryStaffId.HasValue ? src.ShopDeliveryStaff.Account.Email : src.Shop.Account.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(
                src => src.ShopDeliveryStaffId.HasValue ? src.ShopDeliveryStaff.Account.PhoneNumber : src.Shop.Account.PhoneNumber))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(
                src => src.ShopDeliveryStaffId.HasValue ? src.ShopDeliveryStaff.Account.AvatarUrl : src.Shop.Account.AvatarUrl))
            .ForMember(dest => dest.IsShopOwnerShip, opt => opt.MapFrom(
                src => src.ShopId.HasValue));

        CreateMap<Order, ReportForShopWebResponse.CustomerInforInReport>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(
                src => src.CustomerId))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(
                src => src.PhoneNumber))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(
                src => src.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(
                src => src.Customer.Account.Email))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(
                src => src.Customer.Account.AvatarUrl));

        CreateMap<Report, ReportForShopWebResponse>()
            .ForMember(dest => dest.ShopName, opt => opt.MapFrom(
                src => src.Order.Shop.Name))
            .ForMember(dest => dest.Customer, opt => opt.MapFrom(
                src => src.Order))
            .ForMember(dest => dest.ShopDeliveryStaff, opt => opt.MapFrom(
                src => src.Order.DeliveryPackage));
    }

    private long GetDeliveryPackageId(DeliveryPackage dp)
    {
        return dp.ShopDeliveryStaffId.HasValue ? dp.ShopDeliveryStaffId.Value : 0;
    }
}