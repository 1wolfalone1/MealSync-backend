using AutoMapper;
using MealSync.Application.Common.Services.Notifications.Models;
using MealSync.Application.UseCases.Dormitories.Models;
using MealSync.Application.UseCases.Buildings.Models;
using MealSync.Application.UseCases.CustomerBuildings.Models;
using MealSync.Application.UseCases.Customers.Models;
using MealSync.Application.UseCases.Favourites.Models;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Application.UseCases.OptionGroups.Models;
using MealSync.Application.UseCases.Options.Models;
using MealSync.Application.UseCases.Promotions.Models;
using MealSync.Application.UseCases.PlatformCategory.Models;
using MealSync.Application.UseCases.ShopCategories.Models;
using MealSync.Application.UseCases.ShopOwners.Models;
using MealSync.Application.UseCases.Shops.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Dormitory, DormitoryResponse>();
        CreateMap<Building, BuildingResponse>();
        CreateMap<OperatingSlot, ShopOperatingSlotResponse>();
        CreateMap<Location, LocationResponse>();
        CreateMap<ShopDormitory, ShopDormitoryResponse>()
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(
                    src => src.Dormitory != default ? src.Dormitory.Name : string.Empty));
        CreateMap<Shop, ShopConfigurationResponse>()
            .ForMember(dest => dest.OperatingSlots,
                opt => opt.MapFrom(
                    src => src.OperatingSlots))
            .ForMember(dest => dest.Location,
                opt => opt.MapFrom(
                    src => src.Location))
            .ForMember(dest => dest.ShopDormitories,
                opt => opt.MapFrom(
                    src => src.ShopDormitories))
            .ForMember(dest => dest.ShopOwnerName,
                opt => opt.MapFrom(
                    src => src.Account != default ? src.Account.FullName : string.Empty));
        CreateMap<Food, FoodDetailResponse>()
            .ForMember(dest => dest.PlatformCategory, opt => opt.MapFrom(src => src.PlatformCategory))
            .ForMember(dest => dest.ShopCategory, opt => opt.MapFrom(src => src.ShopCategory))
            .ForMember(dest => dest.OperatingSlots, opt => opt.MapFrom(src => src.FoodOperatingSlots))
            .ForMember(dest => dest.FoodOptionGroups, opt => opt.MapFrom(src => src.FoodOptionGroups));
        CreateMap<PlatformCategory, FoodDetailResponse.PlatformCategoryResponse>();
        CreateMap<ShopCategory, FoodDetailResponse.ShopCategoryResponse>();
        CreateMap<OperatingSlot, FoodDetailResponse.OperatingSlotResponse>();
        CreateMap<FoodOperatingSlot, FoodDetailResponse.OperatingSlotResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OperatingSlot.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.OperatingSlot.Title))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.OperatingSlot.StartTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.OperatingSlot.EndTime));
        CreateMap<FoodOptionGroup, FoodDetailResponse.FoodOptionGroupResponse>()
            .ForMember(dest => dest.OptionGroup, opt => opt.MapFrom(src => src.OptionGroup));
        CreateMap<OptionGroup, FoodDetailResponse.OptionGroupResponse>()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));
        CreateMap<Option, FoodDetailResponse.OptionResponse>();
        CreateMap<OptionGroup, OptionGroupResponse>()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));
        CreateMap<Option, OptionGroupResponse.OptionResponse>();
        CreateMap<ShopCategory, ShopCategoryResponse>();
        CreateMap<Shop, ShopProfileResponse>()
            .ForMember(dest => dest.ShopOwnerName,
                opt => opt.MapFrom(
                    src => src.Account != default ? src.Account.FullName : string.Empty));
        CreateMap<Shop, ShopSummaryResponse>()
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.TotalReview > 0 ? Math.Round((double)src.TotalRating / src.TotalReview, 1) : 0));
        CreateMap<Order, OrderNotification>();
        CreateMap<Food, FoodSummaryResponse>();
        CreateMap<Shop, ShopFavouriteResponse>();
        CreateMap<Location, ShopInfoResponse.ShopLocationResponse>();
        CreateMap<OperatingSlot, ShopInfoResponse.ShopOperatingSlotResponse>();
        CreateMap<Dormitory, ShopInfoResponse.ShopDormitoryResponse>();
        CreateMap<Shop, ShopInfoResponse>()
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.OperatingSlots, opt => opt.MapFrom(src => src.OperatingSlots))
            .ForMember(dest => dest.Dormitories, opt => opt.MapFrom(src => src.ShopDormitories.Select(sd => sd.Dormitory)));
        CreateMap<Food, ShopFoodResponse.FoodResponse>();
        CreateMap<Promotion, PromotionSummaryResponse>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.AddHours(-7).ToUnixTimeMilliseconds()))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.AddHours(-7).ToUnixTimeMilliseconds()));
        CreateMap<PlatformCategory, PlatformCategoryResponse>();
        CreateMap<CustomerBuilding, CustomerBuildingResponse>()
            .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
            .ForMember(dest => dest.BuildingId, opt => opt.MapFrom(src => src.BuildingId))
            .ForMember(dest => dest.BuildingName, opt => opt.MapFrom(src => src.Building.Name));
        CreateMap<Food, FoodCartSummaryResponse>();
        CreateMap<Account, CustomerInfoResponse>();
        CreateMap<Food, FoodDetailOfShopResponse>()
            .ForMember(dest => dest.OperatingSlots, opt => opt.MapFrom(src => src.FoodOperatingSlots))
            .ForMember(dest => dest.OptionGroups, opt => opt.MapFrom(src => src.FoodOptionGroups));
        CreateMap<FoodOperatingSlot, FoodDetailOfShopResponse.OperatingSlotOfShopResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OperatingSlot.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.OperatingSlot.Title))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.OperatingSlot.StartTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.OperatingSlot.EndTime));
        CreateMap<FoodOptionGroup, FoodDetailOfShopResponse.OptionGroupOfShopResponse>();
        CreateMap<OptionGroup, ShopOptionGroupResponse>()
            .ForMember(dest => dest.NumOfItemLinked, opt => opt.MapFrom(src => src.FoodOptionGroups != default ? src.FoodOptionGroups.Count() : 0))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));
        CreateMap<Option, OptionResponse>();
    }
}