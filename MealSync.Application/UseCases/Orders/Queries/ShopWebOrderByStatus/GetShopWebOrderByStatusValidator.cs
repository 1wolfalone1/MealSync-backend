using FluentValidation;
using MealSync.Application.UseCases.Orders.Queries.ShopOrderByStatus;

namespace MealSync.Application.UseCases.Orders.Queries.ShopWebOrderByStatus;

public class GetShopWebOrderByStatusValidator : AbstractValidator<GetShopOrderByStatusQuery>
{
    public GetShopWebOrderByStatusValidator()
    {
        RuleFor(x => x.Status)
            .ForEach(x => x.IsInEnum().WithMessage("Vui lòng cung cấp danh sách status từ 1 đến 12"));
    }
}