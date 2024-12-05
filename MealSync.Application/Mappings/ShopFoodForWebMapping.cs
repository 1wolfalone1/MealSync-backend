using AutoMapper;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class ShopFoodForWebMapping : Profile
{
    public ShopFoodForWebMapping()
    {
        CreateMap<FoodPackingUnit, ShopFoodWebResponse.FoodPackingUnitForShopFoodWeb>();
        CreateMap<OperatingSlot, ShopFoodWebResponse.OperatingSlotForShopFoodWeb>();
        CreateMap<ShopCategory, ShopFoodWebResponse.ShopCategoryForShopFoodWeb>();
        CreateMap<Food, ShopFoodWebResponse>()
            .ForMember(dest => dest.OperatingSlots, opt => opt.MapFrom(
                src => src.FoodOperatingSlots.Count > 0 ? src.FoodOperatingSlots.Select(f => f.OperatingSlot).ToList() : new List<OperatingSlot>()))
            .ForMember(dest => dest.NumberOfOptionGroupLinked, opt => opt.MapFrom(
                src => src.FoodOptionGroups != null ? src.FoodOptionGroups.Count : 0));
    }
}