using MediatR;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Shared;

namespace MealSync.Application.UseCases.Roles.Commands.UpdateRole;

public class UpdateRoleHandler : ICommandHandler<UpdateRoleCommand, Result>
{
    private readonly IRoleRepository roleRepository;
    private readonly IUnitOfWork unitOfWork;

    public UpdateRoleHandler(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    {
        this.roleRepository = roleRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<Result<Result>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        await this.unitOfWork.BeginTransactionAsync();
        try
        {
            Role role = await this.roleRepository.GetByIdAsync(request.Role.Id);
            role.Name = request.Role.Name;
            this.roleRepository.Update(role);
            await this.unitOfWork.CommitTransactionAsync();
            return Result.Success(Unit.Value);
        }catch
        {
            this.unitOfWork.RollbackTransaction();
            return Result.Failure(new Error("500", "Fail"));
        }
    }
}
