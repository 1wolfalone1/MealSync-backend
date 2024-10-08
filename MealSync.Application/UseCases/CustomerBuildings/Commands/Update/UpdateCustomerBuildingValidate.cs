using FluentValidation;

namespace MealSync.Application.UseCases.CustomerBuildings.Commands.Update;

public class UpdateCustomerBuildingValidate : AbstractValidator<UpdateCustomerBuildingCommand>
{
    public UpdateCustomerBuildingValidate()
    {
        RuleFor(q => q.BuildingId)
            .GreaterThan(0)
            .WithMessage("Building id phải lớn hơn 0");
    }
}