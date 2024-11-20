using AutoMapper;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class FoodMapping : Profile
{
    public FoodMapping()
    {
        CreateMap<Food, FoodDetailForModManageResponse>();
    }
}