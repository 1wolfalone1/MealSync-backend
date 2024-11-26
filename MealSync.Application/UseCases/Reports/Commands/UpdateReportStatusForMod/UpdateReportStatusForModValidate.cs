using FluentValidation;

namespace MealSync.Application.UseCases.Reports.Commands.UpdateReportStatusForMod;

public class UpdateReportStatusForModValidate : AbstractValidator<UpdateReportStatusForModCommand>
{
    public UpdateReportStatusForModValidate()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Report Id phải lớn hơn 0.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status trong UnderReview = 1, Approved = 2, Rejected = 3");
    }
}