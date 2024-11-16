using FluentValidation;

namespace MealSync.Application.UseCases.Foods.Queries.GetFoodReorder;

public class GetFoodReorderQueryValidate : AbstractValidator<GetFoodReorderQuery>
{
    public GetFoodReorderQueryValidate()
    {
        RuleFor(q => q.OrderId)
            .GreaterThan(0)
            .WithMessage("Order id phải lớn hơn 0");

        When(q => !q.IsGetShopInfo, () =>
        {
            RuleFor(q => q.OperatingSlotId)
                .NotNull()
                .WithMessage("OperatingSlotId phải có giá trị khi IsGetShopInfo là false.");

            RuleFor(q => q.IsOrderForNextDay)
                .NotNull()
                .WithMessage("IsOrderForNextDay phải có giá trị khi IsGetShopInfo là false.");

            RuleFor(q => q.BuildingOrderId)
                .NotNull()
                .WithMessage("BuildingOrderId phải có giá trị khi IsGetShopInfo là false.");
        });
    }
}