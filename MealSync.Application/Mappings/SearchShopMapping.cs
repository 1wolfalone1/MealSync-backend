using AutoMapper;
using MealSync.Application.UseCases.Shops.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class SearchShopMapping : Profile
{
    public SearchShopMapping()
    {
        CreateMap<Shop, SearchShopResponse>()
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.TotalReview > 0 ? Math.Round((double)src.TotalRating / src.TotalReview, 1) : 0))
            .ForMember(dest => dest.Foods, opt => opt.MapFrom(src => src.Foods));
        CreateMap<Food, SearchShopResponse.FoodShopResponse>();
    }
}