using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Queries.ShopOrderByStatus;

public class GetShopOrderByStatusValidator : AbstractValidator<GetShopOrderByStatusQuery>
{
    public GetShopOrderByStatusValidator()
    {
        RuleFor(x => x.Status)
            .ForEach(x => x.IsInEnum().WithMessage("Vui lòng cung cấp danh sách status từ 1 đến 11"));
    }
}