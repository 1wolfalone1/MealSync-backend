using MediatR;
using MealSync.Application.Common.Abstractions.Messaging;

namespace MealSync.Application.UseCases.Roles.Commands.CreateRole;

public class CreateRoleCommand : ICommand<Unit>
{
    public string Name { get; set; }

}