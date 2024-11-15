using AutoMapper;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Application.UseCases.Shops.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class ShopMapping : Profile
{
    public ShopMapping()
    {
        CreateMap<Dormitory, ShopInfoReOrderResponse.ShopDormitoryReOrderResponse>();
        CreateMap<Dormitory, FoodReOrderResponse.DormitoryReOrderResponse>();
        CreateMap<Location, FoodReOrderResponse.LocationResponse>();
    }
}