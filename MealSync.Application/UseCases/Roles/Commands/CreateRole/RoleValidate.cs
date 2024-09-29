using FluentValidation;
using MealSync.Domain.Entities;

namespace MealSync.Application.UseCases.Roles.Commands.CreateRole;

public class RoleValidate : AbstractValidator<Role>
{
    public RoleValidate()
    {
        RuleFor(x => x.Name).NotEmpty().NotNull();
    }
}