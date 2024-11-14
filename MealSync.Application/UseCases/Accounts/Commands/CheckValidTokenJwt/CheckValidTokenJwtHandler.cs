using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Accounts.Commands.CheckValidTokenJwt;

public class CheckValidTokenJwtHandler : ICommandHandler<CheckValidTokenJwtCommand, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public CheckValidTokenJwtHandler(ICurrentPrincipalService currentPrincipalService)
    {
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(CheckValidTokenJwtCommand request, CancellationToken cancellationToken)
    {
        if (request.Role.HasValue)
        {
            if (request.Role.Value.GetDescription() != _currentPrincipalService.CurrentPrincipalRoleName)
                return Result.Failure(new Error("403", "Authorization failed: Bạn không có quyền truy cập"));
        }

        return Result.Success();
    }
}