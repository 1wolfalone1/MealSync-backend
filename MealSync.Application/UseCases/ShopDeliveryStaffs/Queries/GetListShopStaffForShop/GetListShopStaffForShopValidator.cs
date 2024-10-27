using FluentValidation;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Queries.GetListShopStaffForShop;

public class GetListShopStaffForShopValidator : AbstractValidator<GetListShopStaffForShopQuery>
{
    public GetListShopStaffForShopValidator()
    {
        RuleFor(x => x.OrderByMode)
            .Must(x => x >= 0 && x <= 1)
            .WithMessage("Vui lòng chọn 0 hoặc 1 cho order by mode");
    }
}