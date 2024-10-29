using AutoMapper;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class ShopDeliveryStaffInShopOrderDetailMapping : Profile
{
    public ShopDeliveryStaffInShopOrderDetailMapping()
    {
        CreateMap<DeliveryPackage, OrderDetailForShopResponse.ShopDeliveryStaffInShopOrderDetail>()
            .ForMember(dest => dest.DeliveryPackageId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ShopDeliveryStaffId.HasValue && src.ShopDeliveryStaff != null ? src.ShopDeliveryStaff.Id : 0))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.ShopDeliveryStaff != null && src.ShopDeliveryStaff.Account != null ? src.ShopDeliveryStaff.Account.AvatarUrl : null))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.ShopDeliveryStaff != null && src.ShopDeliveryStaff.Account != null ? src.ShopDeliveryStaff.Account.Email : null))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.ShopDeliveryStaff != null && src.ShopDeliveryStaff.Account != null ? src.ShopDeliveryStaff.Account.FullName : null))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.ShopDeliveryStaff != null && src.ShopDeliveryStaff.Account != null ? src.ShopDeliveryStaff.Account.PhoneNumber : null))
            .ForMember(dest => dest.IsShopOwnerShip, opt => opt.MapFrom(src => !src.ShopDeliveryStaffId.HasValue && src.Shop != null));
    }
}