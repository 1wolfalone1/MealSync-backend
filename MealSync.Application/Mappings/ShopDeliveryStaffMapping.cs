using AutoMapper;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class ShopDeliveryStaffMapping : Profile
{
    public ShopDeliveryStaffMapping()
    {
        CreateMap<Account, ShopDeliveryStaffInfoResponse>()
            .ForMember(dest => dest.AccountStatus, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.ShopDeliveryStaffStatus, opt => opt.MapFrom(src => src.ShopDeliveryStaff!.Status));
    }
}