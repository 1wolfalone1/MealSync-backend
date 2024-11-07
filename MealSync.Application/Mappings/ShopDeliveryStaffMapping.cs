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

        CreateMap<ShopDeliveryStaff, ShopDeliveryStaffInfoResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account.PhoneNumber))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.Account.AvatarUrl))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Account.FullName))
            .ForMember(dest => dest.Genders, opt => opt.MapFrom(src => src.Account.Genders))
            .ForMember(dest => dest.AccountStatus, opt => opt.MapFrom(src => src.Account.Status))
            .ForMember(dest => dest.ShopDeliveryStaffStatus, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate));
    }
}