using FluentValidation;
using MediatR;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.UseCases.Roles.Commands.CreateRole;
using MealSync.Domain.Entities;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Roles.Commands.CreateRole;

public class CreateRoleHandler : ICommandHandler<CreateRoleCommand, Unit>
{
    private readonly IRoleRepository roleRepository;
    private readonly IUnitOfWork unitOfWork;


    public CreateRoleHandler(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    {
        this.roleRepository = roleRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = new Role();
        role.Name = request.Name;
        await unitOfWork.BeginTransactionAsync();
        await roleRepository.AddAsync(role);
        await unitOfWork.CommitTransactionAsync();

        return Result.Success(Unit.Value);
    }
}

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().NotNull();
    }
}