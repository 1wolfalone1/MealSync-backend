using AutoMapper;
using MealSync.Application.UseCases.FoodPackingUnits.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class ShopCreateFoodPackingUnitMapping : Profile
{
    public ShopCreateFoodPackingUnitMapping()
    {
        CreateMap<FoodPackingUnit, FoodPackingUnitResponse>();
    }
}