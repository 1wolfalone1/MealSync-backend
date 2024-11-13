using AutoMapper;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class EvidenceOrderMapping : Profile
{
    public EvidenceOrderMapping()
    {
        CreateMap<Order, EvidenceOrderResponse>();
    }
}