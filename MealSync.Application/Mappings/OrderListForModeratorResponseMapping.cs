using AutoMapper;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class OrderListForModeratorResponseMapping : Profile
{
    public OrderListForModeratorResponseMapping()
    {
        CreateMap<Building, OrderListForModeratorResponse.BuildingOrderList>();
        CreateMap<Dormitory, OrderListForModeratorResponse.DormitoryOrderList>();
        CreateMap<Shop, OrderListForModeratorResponse.ShopInforOrderList>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(
                src => src.Account != null ? src.Account.FullName : string.Empty));
        CreateMap<Order, OrderListForModeratorResponse>()
            .ForMember(dest => dest.Buidling, opt => opt.MapFrom(
                src => src.Building))
            .ForMember(dest => dest.Dormitory, opt => opt.MapFrom(
                src => src.Building.Dormitory))
            .ForMember(dest => dest.Shop, opt => opt.MapFrom(
                src => src.Shop));
    }
}