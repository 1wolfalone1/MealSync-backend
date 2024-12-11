using AutoMapper;
using MealSync.Application.UseCases.Moderators.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class ModeratorActivityLogResponseMapping : Profile
{
    public ModeratorActivityLogResponseMapping()
    {
        CreateMap<Account, ModeratorActivityLogResponse.AccountInActivityLog>();
        CreateMap<ActivityLog, ModeratorActivityLogResponse>();
    }
}