using System.Text.Json;
using AutoMapper;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Services.Notifications.Models;
using MealSync.Application.Common.Utils;
using MealSync.Application.UseCases.Dormitories.Models;
using MealSync.Application.UseCases.Buildings.Models;
using MealSync.Application.UseCases.CustomerBuildings.Models;
using MealSync.Application.UseCases.Customers.Models;
using MealSync.Application.UseCases.Favourites.Models;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Application.UseCases.OperatingSlots.Models;
using MealSync.Application.UseCases.OptionGroups.Models;
using MealSync.Application.UseCases.Options.Models;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Application.UseCases.Promotions.Models;
using MealSync.Application.UseCases.PlatformCategory.Models;
using MealSync.Application.UseCases.Reviews.Models;
using MealSync.Application.UseCases.ShopCategories.Models;
using MealSync.Application.UseCases.ShopOwners.Models;
using MealSync.Application.UseCases.Shops.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Dormitory, DormitoryResponse>();
        CreateMap<Building, BuildingResponse>();
        CreateMap<OperatingSlot, ShopOperatingSlotResponse>();
        CreateMap<Location, LocationResponse>();
        CreateMap<ShopDormitory, ShopDormitoryResponse>()
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(
                    src => src.Dormitory != default ? src.Dormitory.Name : string.Empty));
        CreateMap<Shop, ShopConfigurationResponse>()
            .ForMember(dest => dest.OperatingSlots,
                opt => opt.MapFrom(
                    src => src.OperatingSlots))
            .ForMember(dest => dest.Location,
                opt => opt.MapFrom(
                    src => src.Location))
            .ForMember(dest => dest.ShopDormitories,
                opt => opt.MapFrom(
                    src => src.ShopDormitories))
            .ForMember(dest => dest.ShopOwnerName,
                opt => opt.MapFrom(
                    src => src.Account != default ? src.Account.FullName : string.Empty))
            .ForMember(dest => dest.Email,
                opt => opt.MapFrom(
                    src => src.Account != default ? src.Account.Email : string.Empty))
            .ForMember(dest => dest.ShopOwnerAvatar,
                opt => opt.MapFrom(
                    src => src.Account != default ? src.Account.AvatarUrl : string.Empty))
            .ForMember(dest => dest._maxOrderHoursInAdvande, opt => opt.MapFrom(
                src => src.MaxOrderHoursInAdvance))
            .ForMember(dest => dest._minOrderHoursInAdvande, opt => opt.MapFrom(
                src => src.MinOrderHoursInAdvance));
        CreateMap<Food, FoodDetailResponse>()
            .ForMember(dest => dest.PlatformCategory, opt => opt.MapFrom(src => src.PlatformCategory))
            .ForMember(dest => dest.ShopCategory, opt => opt.MapFrom(src => src.ShopCategory))
            .ForMember(dest => dest.OperatingSlots, opt => opt.MapFrom(src => src.FoodOperatingSlots))
            .ForMember(dest => dest.OptionGroups, opt => opt.MapFrom(src => src.FoodOptionGroups));
        CreateMap<PlatformCategory, FoodDetailResponse.PlatformCategoryResponse>();
        CreateMap<ShopCategory, FoodDetailResponse.ShopCategoryResponse>();
        CreateMap<OperatingSlot, FoodDetailResponse.OperatingSlotResponse>();
        CreateMap<FoodOperatingSlot, FoodDetailResponse.OperatingSlotResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OperatingSlot.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.OperatingSlot.Title))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.OperatingSlot.StartTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.OperatingSlot.EndTime))
            .ForMember(dest => dest.IsReceivingOrderPaused, opt => opt.MapFrom(src => src.OperatingSlot.IsReceivingOrderPaused));
        CreateMap<FoodOptionGroup, FoodDetailResponse.FoodOptionGroupResponse>()
            .ForMember(dest => dest.OptionGroup, opt => opt.MapFrom(src => src.OptionGroup));
        CreateMap<OptionGroup, FoodDetailResponse.OptionGroupResponse>()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));
        CreateMap<Option, FoodDetailResponse.OptionResponse>();
        CreateMap<OptionGroup, OptionGroupResponse>()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));
        CreateMap<Option, OptionGroupResponse.OptionResponse>();
        CreateMap<ShopCategory, ShopCategoryResponse>();
        CreateMap<Shop, ShopProfileResponse>()
            .ForMember(dest => dest.ShopOwnerName,
                opt => opt.MapFrom(
                    src => src.Account != default ? src.Account.FullName : string.Empty));
        CreateMap<Shop, ShopSummaryResponse>()
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.TotalReview > 0 ? Math.Round((double)src.TotalRating / src.TotalReview, 1) : 0));
        CreateMap<Order, OrderNotification>();
        CreateMap<Food, FoodSummaryResponse>();
        CreateMap<Shop, ShopFavouriteResponse>();
        CreateMap<Location, ShopInfoResponse.ShopLocationResponse>();
        CreateMap<OperatingSlot, ShopInfoResponse.ShopOperatingSlotResponse>();
        CreateMap<Dormitory, ShopInfoResponse.ShopDormitoryResponse>();
        CreateMap<Shop, ShopInfoResponse>()
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.TotalReview > 0 ? Math.Round((double)src.TotalRating / src.TotalReview, 1) : 0))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.OperatingSlots, opt => opt.MapFrom(src => src.OperatingSlots))
            .ForMember(dest => dest.Dormitories, opt => opt.MapFrom(src => src.ShopDormitories.Select(sd => sd.Dormitory)));
        CreateMap<Food, ShopFoodResponse.FoodResponse>();
        CreateMap<Promotion, PromotionSummaryResponse>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToUnixTimeMilliseconds()))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ToUnixTimeMilliseconds()));
        CreateMap<PlatformCategory, PlatformCategoryResponse>();
        CreateMap<CustomerBuilding, CustomerBuildingResponse>()
            .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
            .ForMember(dest => dest.BuildingId, opt => opt.MapFrom(src => src.BuildingId))
            .ForMember(dest => dest.BuildingName, opt => opt.MapFrom(src => src.Building.Name))
            .ForMember(dest => dest.DormitoryId, opt => opt.MapFrom(src => src.Building.DormitoryId));
        CreateMap<Food, FoodCartSummaryResponse>();
        CreateMap<Account, CustomerInfoResponse>();
        CreateMap<Food, FoodDetailOfShopResponse>()
            .ForMember(dest => dest.OperatingSlots, opt => opt.MapFrom(src => src.FoodOperatingSlots))
            .ForMember(dest => dest.OptionGroups, opt => opt.MapFrom(src => src.FoodOptionGroups));
        CreateMap<FoodOperatingSlot, FoodDetailOfShopResponse.OperatingSlotOfShopResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OperatingSlot.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.OperatingSlot.Title))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.OperatingSlot.StartTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.OperatingSlot.EndTime));
        CreateMap<FoodOptionGroup, FoodDetailOfShopResponse.OptionGroupOfShopResponse>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.OptionGroup != null ? src.OptionGroup.Title : string.Empty));
        CreateMap<OptionGroup, ShopOptionGroupResponse>()
            .ForMember(dest => dest.NumOfItemLinked, opt => opt.MapFrom(src => src.FoodOptionGroups != default ? src.FoodOptionGroups.Count() : 0))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));
        CreateMap<Option, OptionResponse>();
        CreateMap<Food, ShopOwnerFoodResponse.FoodResponse>();
        CreateMap<OperatingSlot, OperatingSlotResponse>();

        CreateMap<Order, OrderResponse>()
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate.ToUnixTimeMilliseconds()))
            .ForMember(
                dest => dest.IntendedReceiveDate,
                opt => opt.MapFrom(src =>
                    new DateTimeOffset(src.IntendedReceiveDate, TimeSpan.Zero).ToUnixTimeSeconds())
            )
            .ForMember(dest => dest.ReceiveAt, opt => opt.MapFrom(src => src.ReceiveAt != default ? src.ReceiveAt.Value.ToUnixTimeMilliseconds() : default))
            .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.CompletedAt != default ? src.CompletedAt.Value.ToUnixTimeMilliseconds() : default))
            .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails));
        CreateMap<OrderDetail, OrderResponse.OrderDetailResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Food.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Food.Name))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Food.ImageUrl))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.BasicPrice, opt => opt.MapFrom(src => src.BasicPrice))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
            .ForMember(
                dest => dest.OptionGroups,
                opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Description)
                    ? null
                    : JsonSerializer.Deserialize<List<OrderDetailDescriptionDto>>(
                        src.Description,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                )
            );

        CreateMap<Order, DetailOrderCustomerResponse>()
            .ForMember(dest => dest.IsReviewAllowed, opt => opt.MapFrom(src => IsReviewAllowed(src)))
            .ForMember(dest => dest.IsReportAllowed, opt => opt.MapFrom(src => IsReportAllowed(src)))
            .ForMember(dest => dest.IsAlreadyReport, opt => opt.MapFrom(src => src.Reports.Count > 0))
            .ForMember(dest => dest.IsCancelAllowed, opt => opt.MapFrom(src => IsCancelAllowed(src)))
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate.ToUnixTimeMilliseconds()))
            .ForMember(
                dest => dest.IntendedReceiveDate,
                opt => opt.MapFrom(src =>
                    new DateTimeOffset(src.IntendedReceiveDate, TimeSpan.Zero).ToUnixTimeSeconds())
            )
            .ForMember(dest => dest.IsOrderNextDay, opt => opt.MapFrom(src => src.OrderDate.Day != src.IntendedReceiveDate.Day))
            .ForMember(dest => dest.ReceiveAt, opt => opt.MapFrom(src => src.ReceiveAt != default ? src.ReceiveAt.Value.ToUnixTimeMilliseconds() : default))
            .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.CompletedAt != default ? src.CompletedAt.Value.ToUnixTimeMilliseconds() : default))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.CustomerLocation.Longitude))
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.CustomerLocation.Latitude))
            .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails))
            .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments.FirstOrDefault(p => p.Type == PaymentTypes.Payment)))
            .ForMember(dest => dest.ShopInfo, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.Promotion, opt => opt.MapFrom(src => src.Promotion));
        CreateMap<OrderDetail, DetailOrderCustomerResponse.OrderDetailCustomerResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Food.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Food.Name))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Food.ImageUrl))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.BasicPrice, opt => opt.MapFrom(src => src.BasicPrice))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
            .ForMember(
                dest => dest.OptionGroups,
                opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Description)
                    ? null
                    : JsonSerializer.Deserialize<List<OrderDetailDescriptionDto>>(
                        src.Description,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                )
            );
        CreateMap<Payment, DetailOrderCustomerResponse.PaymentOrderResponse>();
        CreateMap<Order, DetailOrderCustomerResponse.ShopInfoResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Shop.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Shop.Name))
            .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.Shop.LogoUrl))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.ShopLocation.Address))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.ShopLocation.Longitude))
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.ShopLocation.Latitude));

        CreateMap<Promotion, DetailOrderCustomerResponse.PromotionOrderResponse>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToUnixTimeMilliseconds()))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ToUnixTimeMilliseconds()));

        CreateMap<Order, OrderSummaryResponse>()
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate.ToUnixTimeMilliseconds()))
            .ForMember(
                dest => dest.IntendedReceiveDate,
                opt => opt.MapFrom(src =>
                    new DateTimeOffset(src.IntendedReceiveDate, TimeSpan.Zero).ToUnixTimeSeconds())
            )
            // .ForMember(dest => dest.TotalOrderDetail, opt => opt.MapFrom(src => src.OrderDetails.Count))
            .ForMember(dest => dest.ShopName, opt => opt.MapFrom(src => src.Shop.Name))
            .ForMember(dest => dest.ShopLogoUrl, opt => opt.MapFrom(src => src.Shop.LogoUrl));

        CreateMap<OrderSummaryDto, OrderSummaryResponse>()
            .ForMember(
                dest => dest.OrderDate,
                opt => opt.MapFrom(src =>
                    new DateTimeOffset(src.OrderDate, TimeSpan.Zero).ToUnixTimeSeconds())
            )
            .ForMember(
                dest => dest.IntendedReceiveDate,
                opt => opt.MapFrom(src =>
                    new DateTimeOffset(src.IntendedReceiveDate, TimeSpan.Zero).ToUnixTimeSeconds())
            );

        CreateMap<Food, ShopCategoryDetailResponse.ShopCategoryFoodResponse>();
        CreateMap<ShopCategory, ShopCategoryDetailResponse>()
            .ForMember(dest => dest.Foods, src => src.MapFrom(opt => opt.Foods));

        CreateMap<Review, ReviewDetailResponse>()
            .ForMember(
                dest => dest.ImageUrls,
                src => src.MapFrom(opt =>
                    string.IsNullOrEmpty(opt.ImageUrl)
                        ? new List<string>()
                        : opt.ImageUrl.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()));

        CreateMap<Promotion, PromotionDetailOfShop>()
            .ForMember(dest => dest.IsAvailable, src => src.MapFrom(opt => IsAvailablePromotion(opt)));
    }

    private bool IsAvailablePromotion(Promotion promotion)
    {
        var now = DateTimeOffset.UtcNow;
        if (promotion.Status == PromotionStatus.UnActive)
        {
            return false;
        }
        else if (promotion.NumberOfUsed >= promotion.UsageLimit)
        {
            return false;
        }
        else if (promotion.EndDate < now)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool IsReviewAllowed(Order order)
    {
        var now = DateTimeOffset.UtcNow;

        var receiveDateStartTime = new DateTime(
            order.IntendedReceiveDate.Year,
            order.IntendedReceiveDate.Month,
            order.IntendedReceiveDate.Day,
            order.StartTime / 100,
            order.StartTime % 100,
            0);
        DateTime receiveDateEndTime;
        if (order.EndTime == 2400)
        {
            receiveDateEndTime = new DateTime(
                    order.IntendedReceiveDate.Year,
                    order.IntendedReceiveDate.Month,
                    order.IntendedReceiveDate.Day,
                    0,
                    0,
                    0)
                .AddDays(1);
        }
        else
        {
            receiveDateEndTime = new DateTime(
                order.IntendedReceiveDate.Year,
                order.IntendedReceiveDate.Month,
                order.IntendedReceiveDate.Day,
                order.EndTime / 100,
                order.EndTime % 100,
                0);
        }

        var startTime = new DateTimeOffset(receiveDateStartTime, TimeSpan.FromHours(7));
        var endTime = new DateTimeOffset(receiveDateEndTime, TimeSpan.FromHours(7));

        return (order.Status == OrderStatus.Delivered ||
                ((order.Status == OrderStatus.IssueReported || order.Status == OrderStatus.UnderReview || order.Status == OrderStatus.Resolved)
                 && order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription() && order.IsReport)
                || (order.Status == OrderStatus.Completed && order.ReasonIdentity == default))
               && order.Reviews.Count == 0 && now >= startTime && now <= endTime.AddHours(24);
    }

    private bool IsReportAllowed(Order order)
    {
        var now = DateTimeOffset.UtcNow;
        var receiveDateStartTime = new DateTime(
            order.IntendedReceiveDate.Year,
            order.IntendedReceiveDate.Month,
            order.IntendedReceiveDate.Day,
            order.StartTime / 100,
            order.StartTime % 100,
            0);
        DateTime receiveDateEndTime;
        if (order.EndTime == 2400)
        {
            receiveDateEndTime = new DateTime(
                    order.IntendedReceiveDate.Year,
                    order.IntendedReceiveDate.Month,
                    order.IntendedReceiveDate.Day,
                    0,
                    0,
                    0)
                .AddDays(1);
        }
        else
        {
            receiveDateEndTime = new DateTime(
                order.IntendedReceiveDate.Year,
                order.IntendedReceiveDate.Month,
                order.IntendedReceiveDate.Day,
                order.EndTime / 100,
                order.EndTime % 100,
                0);
        }

        var startTime = new DateTimeOffset(receiveDateStartTime, TimeSpan.FromHours(7));
        var endTime = new DateTimeOffset(receiveDateEndTime, TimeSpan.FromHours(7));

        return (
                   (order.Status == OrderStatus.FailDelivery
                    && order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription())
                   || order.Status == OrderStatus.Delivered
               )
               && order.Reports.Count == 0 && now >= startTime && now <= endTime.AddHours(12);
    }

    private bool IsCancelAllowed(Order order)
    {
        var now = DateTimeOffset.UtcNow;
        var intendedReceiveDateTime = new DateTime(
            order.IntendedReceiveDate.Year,
            order.IntendedReceiveDate.Month,
            order.IntendedReceiveDate.Day,
            order.StartTime / 100,
            order.StartTime % 100,
            0);
        var endTime = new DateTimeOffset(intendedReceiveDateTime, TimeSpan.FromHours(7)).AddHours(-TimeUtils.TIME_CANCEL_ORDER_CONFIRMED_IN_HOURS);

        return order.Status == OrderStatus.Pending || (order.Status == OrderStatus.Confirmed && now < endTime);
    }
}