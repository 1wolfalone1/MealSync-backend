using AutoMapper;
using MealSync.Application.Common.Utils;
using MealSync.Application.UseCases.DeliveryPackages.Models;

namespace MealSync.Application.Mappings;

public class TimeFrameResponseMapping : Profile
{
    public TimeFrameResponseMapping()
    {
        CreateMap<(int StartTime, int EndTime, int NumberOfOrder, bool IsCreated), TimeFrameResponse>()
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => TimeFrameUtils.ConvertEndTime(src.EndTime)))
            .ForMember(dest => dest.NumberOfOrder, opt => opt.MapFrom(src => src.NumberOfOrder))
            .ForMember(dest => dest.IsCreated, opt => opt.MapFrom(src => src.IsCreated));
    }
}