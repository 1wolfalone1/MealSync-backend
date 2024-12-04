using FluentValidation;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.Shops.Queries.SearchShop;

public class SearchShopValidate : AbstractValidator<SearchShopQuery>
{
    public SearchShopValidate()
    {
        When(query => query.PlatformCategoryId.HasValue, () =>
        {
            RuleFor(x => x.PlatformCategoryId!.Value)
                .GreaterThan(0)
                .WithMessage("Id thể loại phải lớn hơn 0.");
        });

        When(query => query.StartTime.HasValue, () =>
        {
            RuleFor(x => x.StartTime!.Value)
                .Must(TimeUtils.IsValidOperatingSlot)
                .WithMessage("Vui lòng cung cấp thời gian bắt đầu đúng định dạng hhMM.");
        });

        When(query => query.EndTime.HasValue, () =>
        {
            RuleFor(x => x.EndTime!.Value)
                .Must(TimeUtils.IsValidOperatingSlot)
                .WithMessage("Vui lòng cung cấp thời gian kết thúc đúng định dạng hhMM.");
        });

        RuleFor(query => query.FoodSize)
            .GreaterThan(0)
            .WithMessage("Tổng số đồ ăn trên shop phải lớn hơn 0.");

        RuleFor(query => query.Order)
            .IsInEnum()
            .When(query => query.Order.HasValue)
            .WithMessage("Sắp xếp theo 1(Giá) 2 (Đánh giá).");

        RuleFor(query => query.Direct)
            .IsInEnum()
            .When(query => query.Direct != default)
            .WithMessage("Sắp xếp theo 1(Tăng dần) 2(Giảm dần).");
    }
}