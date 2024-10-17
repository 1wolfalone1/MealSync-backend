using AutoMapper;
using MealSync.Application.UseCases.OptionGroups.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class OptionGroupDetailMapping : Profile
{
    public OptionGroupDetailMapping()
    {
        CreateMap<Option, OptionGroupDetailResponse.OptionResponse>();
        CreateMap<Food, OptionGroupDetailResponse.FoodInOptionGroupResponse>();
        CreateMap<OptionGroup, OptionGroupDetailResponse>()
            .ForMember(dest => dest.Foods, opt => opt.MapFrom(
                src => src.FoodOptionGroups != null ? src.FoodOptionGroups.Select(x => x.Food).ToList() : new List<Food>()));
    }
}