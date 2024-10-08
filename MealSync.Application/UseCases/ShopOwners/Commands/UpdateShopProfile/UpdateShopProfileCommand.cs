using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopProfile;

public class UpdateShopProfileCommand : ICommand<Result>
{
    public string ShopName { get; set; }

    public string ShopOnwerName { get; set; }

    public long[] DormitoryIds { get; set; }

    public string LogoUrl { get; set; }

    public string BannerUrl { get; set; }

    public string Description { get; set; }

    public string PhoneNumber { get; set; }

    public LocationRequest Location { get; set; }

    public class LocationRequest
    {
        public long Id { get; set; }

        public string Address { get; set; }

        public double Latitude { get; set; }

        public double Longtiude { get; set; }
    }
}

