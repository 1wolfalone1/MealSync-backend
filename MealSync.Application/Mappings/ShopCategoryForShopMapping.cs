using AutoMapper;
using MealSync.Application.UseCases.ShopCategories.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class ShopCategoryForShopMapping : Profile
{
    public ShopCategoryForShopMapping()
    {
        CreateMap<ShopCategory, ShopCategoryForShopWebResponse>()
            .ForMember(dest => dest.NumberFoodLinked, opt => opt.MapFrom(
                src => src.Foods.Count));
    }
}