using FluentValidation;

namespace MealSync.Application.UseCases.Favourites.Commands.MarkFavourite;

public class MarkFavouriteValidate : AbstractValidator<MarkFavouriteCommand>
{
    public MarkFavouriteValidate()
    {
        RuleFor(p => p.ShopId)
            .NotNull()
            .GreaterThan(0)
            .WithMessage("Shop id phải lớn hơn 0.");
    }
}