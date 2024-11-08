using FluentValidation;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.Delete;

public class DeleteDeliveryStaffValidate : AbstractValidator<DeleteDeliveryStaffCommand>
{
    public DeleteDeliveryStaffValidate()
    {
        RuleFor(a => a.Id)
            .GreaterThan(0)
            .WithMessage("Id nhân viên phải lớn hơn 0.");
    }
}