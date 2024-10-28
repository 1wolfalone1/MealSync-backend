using AutoMapper;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class DeliveryPackageResponseMapping : Profile
{
    public DeliveryPackageResponseMapping()
    {
        CreateMap<DeliveryPackage, DeliveryPackageResponse>()
            .ForMember(dest => dest.Orders, opt => opt.MapFrom(src => new List<OrderForShopByStatusResponse>()));
    }
}