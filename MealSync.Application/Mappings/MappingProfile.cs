using AutoMapper;
using MealSync.Application.UseCases.Dormitories.Models;
using MealSync.Application.UseCases.Buildings.Models;
using MealSync.Application.UseCases.Favourites.Models;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Application.UseCases.OptionGroups.Models;
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
            .ForMember(dest => dest.ShopDormitoryies,
                opt => opt.MapFrom(
                    src => src.ShopDormitories));
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
        CreateMap<Shop, ShopProfileResponse>();
        CreateMap<Shop, ShopSummaryResponse>()
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.TotalReview > 0 ? Math.Round((double)src.TotalRating / src.TotalReview, 1) : 0));
        CreateMap<Food, FoodSummaryResponse>();
        CreateMap<Shop, ShopFavouriteResponse>();
    }
}