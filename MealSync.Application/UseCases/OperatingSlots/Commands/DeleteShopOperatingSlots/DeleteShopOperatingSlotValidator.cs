using FluentValidation;

namespace MealSync.Application.UseCases.OperatingSlots.Commands.DeleteShopOperatingSlots;

public class DeleteShopOperatingSlotValidator : AbstractValidator<DeleteShopOperatingSlotCommand>
{
    public DeleteShopOperatingSlotValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của khung thời gian");
    }
}