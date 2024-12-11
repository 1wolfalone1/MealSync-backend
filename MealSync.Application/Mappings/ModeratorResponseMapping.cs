using AutoMapper;
using MealSync.Application.UseCases.Moderators.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class ModeratorResponseMapping : Profile
{
    public ModeratorResponseMapping()
    {
        CreateMap<ModeratorDormitory, ModeratorResponse.DormitoryInModerator>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(
                src => src.Dormitory != null ? src.Dormitory.Id : 0))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(
                src => src.Dormitory != null ? src.Dormitory.Name : string.Empty));
        CreateMap<Moderator, ModeratorResponse>()
            .ForMember(dest => dest.Dormitories, opt => opt.MapFrom(
                src => src.ModeratorDormitories))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(
                src => src.Account != default ? src.Account.Status : 0))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(
                src => src.Account != default ? src.Account.PhoneNumber : string.Empty))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(
                src => src.Account != default ? src.Account.AvatarUrl : string.Empty))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(
                src => src.Account != default ? src.Account.Email : string.Empty))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(
                src => src.Account != default ? src.Account.FullName : string.Empty));
    }
}