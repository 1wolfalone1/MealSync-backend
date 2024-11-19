using AutoMapper;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Application.UseCases.Shops.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class ShopMapping : Profile
{
    public ShopMapping()
    {
        CreateMap<Dormitory, ShopInfoReOrderResponse.ShopDormitoryReOrderResponse>();
        CreateMap<Dormitory, FoodReOrderResponse.DormitoryReOrderResponse>();
        CreateMap<Location, FoodReOrderResponse.LocationResponse>();

        CreateMap<Shop, ShopManageDetailResponse>()
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.TotalReview > 0 ? Math.Round((double)src.TotalRating / src.TotalReview, 1) : 0))
            .ForMember(dest => dest.AccountShop, opt => opt.MapFrom(src => src.Account))
            .ForMember(dest => dest.LocationShop, opt => opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.ShopDormitories, opt => opt.MapFrom(src => src.ShopDormitories))
            .ForMember(dest => dest.ShopOperatingSlots, opt => opt.MapFrom(src => src.OperatingSlots));
        CreateMap<Account, ShopManageDetailResponse.AccountShopResponse>();
        CreateMap<Location, ShopManageDetailResponse.LocationShopResponse>();
        CreateMap<ShopDormitory, ShopManageDetailResponse.ShopDormitoryResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DormitoryId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Dormitory.Name));
        CreateMap<OperatingSlot, ShopManageDetailResponse.ShopOperatingSlotResponse>();
    }
}