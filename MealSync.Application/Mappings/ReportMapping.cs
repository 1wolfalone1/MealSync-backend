using AutoMapper;
using MealSync.Application.UseCases.Reports.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class ReportMapping : Profile
{
    public ReportMapping()
    {
        CreateMap<Report, ReportDetailResponse>()
            .ForMember(dest => dest.IsReportedByCustomer, src => src.MapFrom(opt => opt.CustomerId != default && opt.CustomerId > 0))
            .ForMember(
                dest => dest.ImageUrls,
                src => src.MapFrom(opt =>
                    string.IsNullOrEmpty(opt.ImageUrl)
                        ? new List<string>()
                        : opt.ImageUrl.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()));
    }
}