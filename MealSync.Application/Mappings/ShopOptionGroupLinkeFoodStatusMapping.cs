using AutoMapper;
using MealSync.Application.UseCases.OptionGroups.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class ShopOptionGroupLinkeFoodStatusMapping : Profile
{
    public ShopOptionGroupLinkeFoodStatusMapping()
    {
        CreateMap<OptionGroup, ShopOptionGroupLinkeFoodStatusResponse>()
            .ForMember(dest => dest.IdLinked, opt => opt.MapFrom(src => src.FoodOptionGroups != null ? src.FoodOptionGroups.Count > 0 : false));
    }
}