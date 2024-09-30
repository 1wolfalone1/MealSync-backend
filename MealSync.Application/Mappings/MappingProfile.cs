using AutoMapper;
using MealSync.Application.UseCases.Dormitories.Models;
using MealSync.Application.UseCases.Buildings.Models;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Application.UseCases.ShopOwners.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Dormitory, DormitoryResponse>();
        CreateMap<Building, BuildingResponse>();
        CreateMap<OperatingSlot, OperatingDayResponse>();
        CreateMap<Location, LocationResponse>();
        CreateMap<ShopDormitory, ShopDormitoryResponse>()
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(
                    src => src.Dormitory != default ? src.Dormitory.Name : string.Empty));
        CreateMap<Shop, ShopConfigurationResponse>()
            .ForMember(dest => dest.OperatingDays,
                opt => opt.MapFrom(
                    src => src.OperatingSlots))
            .ForMember(dest => dest.Location,
                opt => opt.MapFrom(
                    src => src.Location))
            .ForMember(dest => dest.ShopDormitoryies,
                opt => opt.MapFrom(
                    src => src.ShopDormitories));
        CreateMap<PlatformCategory, FoodDetailResponse.PlatformCategoryResponse>();
        CreateMap<ShopCategory, FoodDetailResponse.ShopCategoryResponse>();
        CreateMap<OperatingSlot, FoodDetailResponse.OperatingSlotResponse>();
        CreateMap<FoodOptionGroup, FoodDetailResponse.FoodOptionGroupResponse>()
            .ForMember(dest => dest.OptionGroup, opt => opt.MapFrom(src => src.OptionGroup));
        CreateMap<OptionGroup, FoodDetailResponse.OptionGroupResponse>()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));
        CreateMap<Option, FoodDetailResponse.OptionResponse>();
        CreateMap<Food, FoodDetailResponse>()
            .ForMember(dest => dest.PlatformCategory, opt => opt.MapFrom(src => src.PlatformCategory))
            .ForMember(dest => dest.ShopCategory, opt => opt.MapFrom(src => src.ShopCategory))
            .ForMember(dest => dest.OperatingSlots, opt => opt.MapFrom(src => src.FoodOperatingSlots))
            .ForMember(dest => dest.FoodOptionGroups, opt => opt.MapFrom(src => src.FoodOptionGroups));
    }
}