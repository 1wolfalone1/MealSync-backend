using AutoMapper;
using MealSync.Application.UseCases.FoodPackingUnits.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class FoodPackingUnitAdminResponseMapping : Profile
{
    public FoodPackingUnitAdminResponseMapping()
    {
        CreateMap<FoodPackingUnit, FoodPackingUnitAdminResponse>()
            .ForMember(dest => dest.NumFoodLinked, opt => opt.MapFrom(
                src => src.Foods != null ? src.Foods.Count : 0));
    }
}