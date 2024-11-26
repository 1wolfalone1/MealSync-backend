using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reports.Models;

public class ReportDetailForCusResponse
{
    public List<ReportDetailResponse> Reports { get; set; }

    public ShopInfoDetailResponse ShopInfo { get; set; }

    public class ShopInfoDetailResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string? LogoUrl { get; set; }

        public string? BannerUrl { get; set; }

        public string? Description { get; set; }

        public string PhoneNumber { get; set; } = null!;
    }
}