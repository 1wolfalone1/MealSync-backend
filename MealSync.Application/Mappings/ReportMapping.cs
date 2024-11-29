using AutoMapper;
using MealSync.Application.Common.Services.Notifications.Models;
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

        CreateMap<Report, ReportDetailShopWebResponse>()
            .ForMember(dest => dest.OrderId, src => src.MapFrom(opt => opt.OrderId))
            .ForMember(dest => dest.IsReportedByCustomer, src => src.MapFrom(opt => opt.CustomerId != default && opt.CustomerId > 0))
            .ForMember(
                dest => dest.ImageUrls,
                src => src.MapFrom(opt =>
                    string.IsNullOrEmpty(opt.ImageUrl)
                        ? new List<string>()
                        : opt.ImageUrl.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()));

        CreateMap<Account, ReportDetailShopWebResponse.CustomerInfoResponse>();

        CreateMap<Report, ReportDetailForModResponse.ReportResponse>()
            .ForMember(dest => dest.OrderId, src => src.MapFrom(opt => opt.OrderId))
            .ForMember(dest => dest.IsReportedByCustomer, src => src.MapFrom(opt => opt.CustomerId != default && opt.CustomerId > 0))
            .ForMember(
                dest => dest.ImageUrls,
                src => src.MapFrom(opt =>
                    string.IsNullOrEmpty(opt.ImageUrl)
                        ? new List<string>()
                        : opt.ImageUrl.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()));

        CreateMap<Account, ReportDetailForModResponse.CustomerInfoForModResponse>()
            .ForMember(dest => dest.Status, src => src.MapFrom(opt => opt.Customer!.Status));

        CreateMap<Shop, ReportDetailForModResponse.ShopInfoForModResponse>();

        CreateMap<Shop, ReportDetailForCusResponse.ShopInfoDetailResponse>();

        CreateMap<Report, ReportNotification>();
    }
}