using FluentValidation;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.UpdateStatus;

public class UpdateStatusValidate : AbstractValidator<UpdateStatusCommand>
{
    public UpdateStatusValidate()
    {
        RuleFor(a => a.Id)
            .GreaterThan(0)
            .WithMessage("Id nhân viên phải lớn hơn 0.");

        RuleFor(a => a.Status)
            .IsInEnum()
            .WithMessage("Trạng thái của nhân viên giao hàng là 1(Online), 2(Offline), 3(UnActive).");
    }
}