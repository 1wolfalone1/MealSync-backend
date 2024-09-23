using AutoMapper;
using MealSync.Application.UseCases.Dormitories.Models;
using MealSync.Application.UseCases.Buildings.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Dormitory, DormitoryResponse>();
        CreateMap<Building, BuildingResponse>();
    }
}