using AutoMapper;
using MealSync.Application.Common.Utils;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Mappings;

public class ShopDeliveryStaffAvailableMapping : Profile
{
    public ShopDeliveryStaffAvailableMapping()
    {
        CreateMap<ShopDeliveryStaff, ShopStaffResponse>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account != null ? src.Account.Email : string.Empty))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account != null ? src.Account.PhoneNumber : string.Empty))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.Account != null ? src.Account.AvatarUrl : string.Empty))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Account != null ? src.Account.FullName : string.Empty))
            .ForMember(dest => dest.CurrentDeliveryPackageIds, opt => opt.MapFrom(src => GetCurrentDeliveryPackageIds(src)))
            .ForMember(dest => dest.FutureDeliveryPackageIds, opt => opt.MapFrom(src => GetFutureDeliveryPackageIds(src)));
    }

    public List<long> GetCurrentDeliveryPackageIds(ShopDeliveryStaff src)
    {
        var currentHours = TimeFrameUtils.GetCurrentHours();
        if (src.DeliveryPackages != null && src.DeliveryPackages.Any(dp => dp.Status == DeliveryPackageStatus.OnGoing))
        {
            return src.DeliveryPackages.Where(dp => dp.Status == DeliveryPackageStatus.OnGoing &&
                                                    dp.StartTime <= currentHours && dp.EndTime >= currentHours).Select(dp => dp.Id).ToList();
        }

        return new List<long>();
    }

    public List<long> GetFutureDeliveryPackageIds(ShopDeliveryStaff src)
    {
        var currentHours = TimeFrameUtils.GetCurrentHours();
        if (src.DeliveryPackages != null && src.DeliveryPackages.Any(dp => dp.Status == DeliveryPackageStatus.OnGoing || dp.Status == DeliveryPackageStatus.Created))
        {
            return src.DeliveryPackages.Where(dp => (dp.Status == DeliveryPackageStatus.OnGoing || dp.Status == DeliveryPackageStatus.Created) &&
                dp.StartTime >= currentHours).Select(dp => dp.Id).ToList();
        }

        return new List<long>();
    }
}