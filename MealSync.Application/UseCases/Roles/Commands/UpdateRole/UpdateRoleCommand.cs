using FluentValidation;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.UseCases.Roles.Commands.CreateRole;
using MealSync.Application.UseCases.Roles.Commands.UpdateRole;
using MealSync.Domain.Shared;

namespace MealSync.Application.UseCases.Roles.Commands.UpdateRole
{
    public class UpdateRoleCommand : ICommand<Result>
    {
        public UpdateRole Role { get; set; }
    }
}

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Role.Id).NotNull();
        RuleFor(x => x.Role.Name).NotEmpty();
    }
}
